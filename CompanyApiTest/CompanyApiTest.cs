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
        public async Task Should_return_200_when_get_all_given_nothing()
        {
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");
          
            // When
            var _hm = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            var cp = await _hm.Content.ReadFromJsonAsync<Company>();

            var httpMessage = await httpClient.GetAsync("/api/companies");

            var content = await httpMessage.Content.ReadFromJsonAsync<List< Company>> ();
           
            // Then
            Assert.Equal(HttpStatusCode.OK, httpMessage.StatusCode);
            Assert.Equal(new List<Company>() { cp}, content);
 
        }

        [Fact]
        public async Task Should_return_200_when_get_by_id_given_id()
        {
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest() { Name = "name"};
            var _hm = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            var cp = await _hm.Content.ReadFromJsonAsync<Company>();
            var httpMessage = await httpClient.GetAsync($"/api/companies/{cp.Id}");
            var exp = await httpMessage.Content.ReadFromJsonAsync<Company>();
            Assert.Equal(HttpStatusCode.OK, httpMessage.StatusCode);
            Assert.Equal(cp, exp);
        }

        [Fact]
        public async Task Should_return_404_when_get_by_id_given_not_exist_id()
        {
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest() { Name = "BlueSky Digital Media" };

            var _hm = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            var cp = await _hm.Content.ReadFromJsonAsync<Company>();
            var httpMessage = await httpClient.GetAsync($"/api/companies/{cp.Id}1");
            var exp = await httpMessage.Content.ReadFromJsonAsync<Company>();

            Assert.Equal(HttpStatusCode.NotFound, httpMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_companies_with_status_when_get_page_given_size_index()
        {
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest { Name = "1" };
            var companyGiven2 = new CreateCompanyRequest() { Name = "2" };
            var companyGiven3 = new CreateCompanyRequest() { Name = "3" };
            var companyGiven4 = new CreateCompanyRequest() { Name = "4" };

            var resp1 = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            var resp2 = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven2));
            var resp3 = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven3));
            var resp4 = await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven4));
            var c3Exp = await resp3.Content.ReadFromJsonAsync<Company>();
            var c4Exp = await resp4.Content.ReadFromJsonAsync<Company>();

            var pageSize = 2;
            var pageIndex = 1;
            var response = await httpClient.GetAsync($"/api/companies/{pageSize}&{pageIndex}");
            var cs = await response.Content.ReadFromJsonAsync<List<Company>>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new List<Company>() { c3Exp, c4Exp }, cs);

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
    }
}