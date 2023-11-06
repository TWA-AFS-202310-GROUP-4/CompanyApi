using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace CompanyApiTest
{
    public class CompanyApiTest
    {
        private HttpClient httpClient;

        public CompanyApiTest()
        {
            WebApplicationFactory<Program> webApplicationFactory = new WebApplicationFactory<Program>();
            httpClient = webApplicationFactory.CreateClient();
        }

        [Fact]
        public async Task Should_return_created_company_with_status_201_when_create_cpmoany_given_a_company_name()
        {
            // Given
            Company companyGiven = new Company("BlueSky Digital Media");
            
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies", 
                SerializeObjectToContent(companyGiven)
            );
           
            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Company? companyCreated = await DeserializeTo<Company>(httpResponseMessage);
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);
            Assert.Equal(companyGiven.Name, companyCreated.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_existed_company_name()
        {
            // Given
           
            Company companyGiven = new Company("Company1");

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies", 
                SerializeObjectToContent(companyGiven)
            );
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_company_with_unknown_field()
        {
            // Given
            StringContent content = new StringContent("{\"unknownField\": \"BlueSky Digital Media\"}", Encoding.UTF8, "application/json");
          
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("/api/companies", content);
           
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        private async Task<T?> DeserializeTo<T>(HttpResponseMessage httpResponseMessage)
        {
            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            T? deserializedObject = JsonConvert.DeserializeObject<T>(response);
            return deserializedObject;
        }

        private static StringContent SerializeObjectToContent<T>(T objectGiven)
        {
            return new StringContent(JsonConvert.SerializeObject(objectGiven), Encoding.UTF8, "application/json");
        }

        private async Task ClearDataAsync()
        {
            await httpClient.DeleteAsync("/api/companies");
        }

        [Fact]
        public async Task Should_return_all_companys_when_get_all_company()
        {   
            var response = await httpClient.GetAsync("/api/companies/");
            var companys = await response.Content.ReadFromJsonAsync<List<Company>>();
            Assert.Equal("Company1", companys[0].Name);

        }


        [Fact]
        public async Task Should_return_correct_companys_when_get_company_given_id()
        {
            string id = "1234";
            var httpResponseMessage = await httpClient.GetAsync($"api/companies/{id}");
            var creatCompany = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            Assert.Equal("Company1", creatCompany.Name);
        }

        [Fact]
        public async Task Should_return_pagesize_from_pageindex_result_give_pagesize_and_pageindex_when_get_pages()
        {
            
            int pageIndex = 2;
            int pageSize = 3;

            var response = await httpClient.GetAsync($"api/companies/page?pageSize={pageSize}&pageIndex={pageIndex}");
            var companys = await response.Content.ReadFromJsonAsync<List<Company>>();
            Assert.Equal(pageSize, companys.Count);
            Assert.Equal("Company4", companys[0].Name);

        }
        [Fact]
        public async Task Should_return_update_company_with_status_201_when_update_company_given_id_and_updatemessage()
        {
            
            Company companyGiven = new Company("Google");
            var response =  await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var responsecompany = await  response.Content.ReadFromJsonAsync<Company>();
            var updatecompany = new Company("google");
            var updateResponse =  await httpClient.PutAsJsonAsync($"api/companies/{responsecompany.Id}", updatecompany);
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        }

        [Fact]
        public async Task Should_return_update_company_with_status_201_when_add_employee_to_company_give_employee_and_company_id()
        {
            await ClearDataAsync();
            Company companyGiven = new Company("Company12");
            var response = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var responsecompany = await response.Content.ReadFromJsonAsync<Company>();

            Employee employee = new Employee("wdx", 20);
            var addEmployeeresponse = await httpClient.PostAsJsonAsync($"/api/companies/{responsecompany.Id}/employees", employee);
            var addEmployee = await addEmployeeresponse.Content.ReadFromJsonAsync<Employee>();

            Assert.Equal("wdx", addEmployee.Name);
        }

        [Fact]
        public async Task Should_return_nocontent_when_delete_employee_from_company_given_company_id_and_employee_id()
        {
            await ClearDataAsync();

            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Comapny13");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            var employee = new Employee( "wdx",12);

            HttpResponseMessage httpResponseMessage2 = await httpClient.PostAsJsonAsync($"api/companies/{company.Id}/employees", employee);

            var employee2 = await httpResponseMessage2.Content.ReadFromJsonAsync<Employee>();

            //when
            HttpResponseMessage httpResponseMessageFinal = await httpClient.DeleteAsync($"/api/companies/{company.Id}/employees/{employee2.Id}");

            //then
            Assert.Equal(HttpStatusCode.NoContent, httpResponseMessageFinal.StatusCode);

        }

        [Fact]
        public async Task Should_return_employee_list_when_get_employee_list_given_company_id ()
        {
            await ClearDataAsync();

            CreateCompanyRequest companyGiven = new CreateCompanyRequest("company14");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            var employee1 = new Employee("wdx", 12);
            var employee2 = new Employee("heihei", 12);
            List<Employee> employees1 = new List<Employee>();
            employees1.Add(employee1);
            employees1.Add(employee2);

            await httpClient.PostAsJsonAsync($"api/companies/{company.Id}/employees", employee1);
            await httpClient.PostAsJsonAsync($"api/companies/{company.Id}/employees", employee2);

            var httpResponseMessage2 = await httpClient.GetAsync($"api/companies/{company.Id}/employees");
            var employees2 = await httpResponseMessage2.Content.ReadFromJsonAsync<List<Employee>>();

            Assert.Equal(employees1.Count, employees2.Count);

        }

        [Fact]
        public async Task Should_return_employee_when_update_employee_given_company_id_and_employee_id_update_message()
        {
            await ClearDataAsync();

            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Company15");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            
            var employee1 = new Employee("heihei", 12);
            var httpResponseMessage2 = await httpClient.PostAsJsonAsync($"api/companies/{company.Id}/employees", employee1);
            var employee2 = await httpResponseMessage2.Content.ReadFromJsonAsync<Employee>();

            var updateEmploee = new Employee("heihei", 19);

            var httpResponseMessage3 = await httpClient.PutAsJsonAsync($"api/companies/{company.Id}/employees/{employee2.Id}", updateEmploee);

            Assert.Equal(HttpStatusCode.OK, httpResponseMessage3.StatusCode);
            

        }
        [Fact]
        public async Task Should_return_nocontent_when_delete_company_and_its_employee_given_company_id()
        {
            await ClearDataAsync();

            CreateCompanyRequest companyGiven = new CreateCompanyRequest("Company16");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            var company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();


            var employee1 = new Employee("heihei", 12);
            var httpResponseMessage2 = await httpClient.PostAsJsonAsync($"api/companies/{company.Id}/employees", employee1);
            var httpResponseMessage3 = await httpClient.DeleteAsync($"api/companies/{company.Id}");

            Assert.Equal(HttpStatusCode.NoContent, httpResponseMessage3.StatusCode);


        }


    }
}