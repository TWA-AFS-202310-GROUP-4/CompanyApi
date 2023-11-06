using CompanyApi.Models;
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
            await ClearDataAsync();
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
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");

            // When
            await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
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
            await ClearDataAsync();
            StringContent content = new StringContent("{\"unknownField\": \"BlueSky Digital Media\"}", Encoding.UTF8, "application/json");
          
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("/api/companies", content);
           
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_companies_when_getall_company()
        {
            await ClearDataAsync();
            var createcompaniesGiven = new CreateCompanyRequest("BlueSky Digital Media");
            var _ = await httpClient.PostAsJsonAsync(
                        "/api/companies", createcompaniesGiven);

            var responseMessage = await httpClient.GetAsync("api/companies");
            var companies = await responseMessage.Content.ReadFromJsonAsync<List<Company>>();

            // Then
            Assert.Equal(createcompaniesGiven.Name, companies.FirstOrDefault().Name);
        }

        [Fact]
        public async Task Should_return_givenid_companies_when_ge_company()
        {
            await ClearDataAsync();
            var ccp = new CreateCompanyRequest("hhhh");
            var createResponse = await httpClient.PostAsync(
                "/api/companies",
                SerializeObjectToContent(ccp));
            var cinfo = await createResponse.Content.ReadFromJsonAsync<Company>();
            var responseMessage = await httpClient.GetAsync($"api/companies/{cinfo?.Id}");
            var repCCP = await responseMessage.Content.ReadFromJsonAsync<Company>();

            // Then
            Assert.Equal(cinfo?.Id, repCCP?.Id);
        }

        [Fact]
        public async Task Should_return_none_companies_when_ge_company()
        {
            await ClearDataAsync();
            var ccp = new CreateCompanyRequest("hhhh");
            var createResponse = await httpClient.PostAsync(
                "/api/companies",
                SerializeObjectToContent(ccp));
            var cinfo = await createResponse.Content.ReadFromJsonAsync<Company>();
            var responseMessage = await httpClient.GetAsync($"api/companies/{cinfo?.Id + 10086}");
            var repCCP = await responseMessage.Content.ReadFromJsonAsync<Company>();

            // Then
            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_paged_companies_when_given_pagesize_and_pageindex()
        {
            await ClearDataAsync();
            for (int i = 0; i < 10; i++)
            {
                var _ = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", new CreateCompanyRequest($"hh{i}"));
            }

            var responseInfo = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", new CreateCompanyRequest("hh11"));
            var companyWithId = await responseInfo.Content.ReadFromJsonAsync<Company>();
            var responseInfoSec = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", new CreateCompanyRequest("hh12"));
            var companyWithIdSec = await responseInfoSec.Content.ReadFromJsonAsync<Company>();

            var pageSize = 10;
            var pageIndex = 2;
            var pagedResponse = await httpClient.GetAsync($"/api/companies?pagesize={pageSize}&pageindex={pageIndex}");
            var pageResult = await pagedResponse.Content.ReadFromJsonAsync<List<Company>>();
            Assert.Equal(HttpStatusCode.OK, pagedResponse.StatusCode);
            Assert.Equal(new List<Company>() { companyWithId, companyWithIdSec }, pageResult);
        }

        [Fact]
        public async Task Should_update_company_nameinfo_with_204_when_updated_given_company_id()
        {
            await ClearDataAsync();
            var oldCompany = new CreateCompanyRequest("hh");
            var responseInfo = await httpClient.PostAsJsonAsync<CreateCompanyRequest>("/api/companies", oldCompany);
            var companyWithId = await responseInfo.Content.ReadFromJsonAsync<Company>();
            oldCompany.Name = companyWithId!.Name = "hhh";

            var _ = await httpClient.PutAsJsonAsync($"/api/companies/{companyWithId?.Id}", oldCompany);
            var updatedInfo = await httpClient.GetAsync($"/api/companies/{companyWithId?.Id}");
            var updatedCompany = await updatedInfo.Content.ReadFromJsonAsync<Company>();
            Assert.Equal(companyWithId, updatedCompany);
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
        public async Task Should_return_201_when_create_given_employee()
        {
            await ClearDataAsync();

            var company = new CreateCompanyRequest("google");
            var _ = await httpClient.PostAsJsonAsync("/api/companies", company);
            var employee = new CreateEmployeeRequest("cr", "google", "CEO");
            var createEmployeeResponse = await httpClient.PostAsJsonAsync($"/api/companies/employees", employee);

            Assert.Equal(HttpStatusCode.Created, createEmployeeResponse.StatusCode);
        }

        [Fact]
        public async Task Should_return_204_when_delete_given_employee_id()
        {
            await ClearDataAsync();

            var company = new CreateCompanyRequest("google");
            var _ = await httpClient.PostAsJsonAsync("/api/companies", company);
            var employee = new CreateEmployeeRequest("cr", "google", "CEO");
            var createEmployeeResponse = await httpClient.PostAsJsonAsync($"/api/companies/employees", employee);
            var responseInfo = await createEmployeeResponse.Content.ReadFromJsonAsync<Employee>();

            var deleteResponse = await httpClient.DeleteAsync($"/api/companies/employees/{responseInfo!.Id}");

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }
    }
}