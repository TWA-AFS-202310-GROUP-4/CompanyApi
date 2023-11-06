using CompanyApi;
using CompanyApi.DTO;
using CompanyApi.Request;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Xml.Linq;

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

        [Theory]
        [InlineData(2, "acompany", "bcomoany")]
        public async Task Should_return_all_companies_when_get_all_companies_given_nothing(int expectedLength, params string[] companyname)
        {
            //Given
            await ClearDataAsync();
            foreach (var name in companyname)
            {
                await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest { Name = name });
            }
            //when
            HttpResponseMessage responseMessage = await httpClient.GetAsync("/api/companies");
            List<Company>? companies = await responseMessage.Content.ReadFromJsonAsync<List<Company>>();
            //Then
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
            Assert.NotNull(companies);
            Assert.Equal(expectedLength, companies.Count());
        }

        [Theory]
        [InlineData("acompany")]
        public async Task Should_return_the_correspond_company_when_get_company_by_id_give_the_company_id(string companyname)
        {
            //Given
            await ClearDataAsync();
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest { Name = companyname });
            var existedCompany = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            var id = existedCompany.Id;

            //When
            var url = $"/api/companies/{id}";
            HttpResponseMessage getByIdResponseMessage = await httpClient.GetAsync(url);
            Company? company = await getByIdResponseMessage.Content.ReadFromJsonAsync<Company>();

            //Then
            Assert.NotNull(company);
            Assert.Equal(existedCompany.ToString(), company.ToString());
        }

        [Theory]
        [InlineData("acompany")]
        public async Task Should_get_no_content_when_get_company_by_id_give_not_existed_id(string companyname)
        {
            //Given
            await ClearDataAsync();
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest { Name = companyname });
            var existedCompany = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            var fakeId = Guid.NewGuid().ToString();
            //When
            var url = $"/api/companies/{fakeId}";
            HttpResponseMessage getByIdResponseMessage = await httpClient.GetAsync(url);

            //Then
            Assert.Equal(HttpStatusCode.NoContent, getByIdResponseMessage.StatusCode);
        }

        [Theory]
        [InlineData(8, 2, new string[] { "company1", "company2", "company3", "company4", "company5", "company6", "company7", "company8", "company9", "company10" }, 2)]
        [InlineData(2, 2, new string[] { "company1", "company2", "company3", "company4", "company5", "company6", "company7", "company8", "company9", "company10" }, 2)]
        public async Task Should_get_companies_of_pageIndex_when_get_companies_given_pagesize_and_pageindex(int pageSize, int pageIndex, string[] companiesname, int expectedLength)
        {
            //given
            await ClearDataAsync();
            foreach (var name in companiesname)
            {
                await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest { Name = name });
            }
            //when
            HttpResponseMessage responseMessage = await httpClient.GetAsync($"/api/companies?pageSize={pageSize}&pageIndex={pageIndex}");
            List<Company>? companies = await responseMessage.Content.ReadFromJsonAsync<List<Company>>();

            //then
            Assert.NotNull(companies);
            Assert.Equal(expectedLength, companies.Count);
        }

        [Theory]
        [InlineData(20, 2, new string[] { "company1", "company2", "company3", "company4", "company5", "company6", "company7", "company8", "company9", "company10" })]
        [InlineData(6, 4, new string[] { "company1", "company2", "company3", "company4", "company5", "company6", "company7", "company8", "company9", "company10" })]
        public async Task Should_get_bad_request_of_pageIndex_when_get_companies_given_invalid_pagesize_and_pageindex(int pageSize, int pageIndex, string[] companiesname)
        {
            //given
            await ClearDataAsync();
            foreach (var name in companiesname)
            {
                await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest { Name = name });
            }
            //when
            HttpResponseMessage responseMessage = await httpClient.GetAsync($"/api/companies?pageSize={pageSize}&pageIndex={pageIndex}");

            //then
            Assert.Equal(HttpStatusCode.BadRequest, responseMessage.StatusCode);
        }

        [Theory]
        [InlineData("acompany", "a-update company")]
        public async Task Should_get_updated_company_name_when_update_company_given_existed_company_id(string companyName, string updateCompanyname)
        {
            //given
            await ClearDataAsync();
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest { Name = companyName });
            Company? company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            UpdateCompanyRequest updateCompanyRequest = new UpdateCompanyRequest(company.Id, updateCompanyname);

            //when
            HttpResponseMessage updateHttpResponseMessage = await httpClient.PutAsJsonAsync($"/api/companies/{company.Id}", updateCompanyRequest);
            Company? updateCompany = await updateHttpResponseMessage.Content.ReadFromJsonAsync<Company>();

            //then
            Assert.NotNull(updateCompany);
            Assert.Equal(updateCompanyname, updateCompany.Name);
        }

        [Theory]
        [InlineData("acompany", "a-update company")]
        public async Task Should_get_no_content_when_update_company_given_existed_company_id(string companyName, string updateCompanyname)
        {
            //given
            await ClearDataAsync();
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest { Name = companyName });
            Company? company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>(); UpdateCompanyRequest updateCompanyRequest = new UpdateCompanyRequest(company.Id, updateCompanyname);
            var fakeId = Guid.NewGuid().ToString();

            //when
            HttpResponseMessage updateHttpResponseMessage = await httpClient.PutAsJsonAsync($"/api/companies/{fakeId}", updateCompanyRequest);

            //then
            Assert.Equal(HttpStatusCode.NoContent, updateHttpResponseMessage.StatusCode);
        }

        [Theory]
        [InlineData("Alice")]
        public async Task Should_return_created_employee_with_status_201_when_create_employee_given_a_employee_name_and_companyId(string employeeName)
        {
            // Given
            await ClearDataAsync();
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest
            {
                Name = "BlueSky Digital Media"
            });
            Company? company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            var companyId = company.Id;

            //When
            HttpResponseMessage createEmployeeHttpResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{companyId}/employees", new CreateEmployeeRequest(employeeName));
            Employee? employee = await createEmployeeHttpResponseMessage.Content.ReadFromJsonAsync<Employee>();

            //Then
            Assert.NotNull(employee);
            Assert.Equal(companyId, employee.CompanyId);
        }

        [Theory]
        [InlineData("Alice")]
        public async Task Should_return_not_found_when_create_employee_given_a_employee_name_but_fake_companyId(string employeeName)
        {
            // Given
            await ClearDataAsync();
            var fakeId = Guid.NewGuid().ToString();

            //When
            HttpResponseMessage createEmployeeHttpResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{fakeId}/employees", new CreateEmployeeRequest(employeeName));

            //Then
            Assert.Equal(HttpStatusCode.NotFound, createEmployeeHttpResponseMessage.StatusCode);
        }

        [Theory]
        [InlineData("Alice")]
        public async Task Should_return_no_content_when_delete_employee_given_a_employee_name_and_companyId(string employeeName)
        {
            //given
            await ClearDataAsync();
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest
            {
                Name = "BlueSky Digital Media"
            });
            Company? company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            var companyId = company.Id;
            HttpResponseMessage createEmployeeHttpResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{companyId}/employees", new CreateEmployeeRequest(employeeName));
            Employee? employee = await createEmployeeHttpResponseMessage.Content.ReadFromJsonAsync<Employee>();
            var employeeId = employee.EmployeeId;

            //when
            HttpResponseMessage deleteHttpResponseMessage = await httpClient.DeleteAsync($"/api/companies/{companyId}/employees/{employeeId}");

            //then
            Assert.Equal(HttpStatusCode.NoContent, deleteHttpResponseMessage.StatusCode);
        }
    }
}