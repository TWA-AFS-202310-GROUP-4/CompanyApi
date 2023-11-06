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
        public async Task Should_return_all_companys_when_get_all()
        {

          
            Company company1 = new Company("Google");
            Company company2 = new Company("Baidu");

            var hettpResponse1 = await httpClient.PostAsJsonAsync("/api/companies", company1);
            var hettpResponse2 = await httpClient.PostAsJsonAsync("/api/companies", company2);

           
            var response = await httpClient.GetAsync("/api/companies/");
            var companys = await response.Content.ReadFromJsonAsync<List<Company>>();
            Assert.Equal(HttpStatusCode.OK,response.StatusCode);

        }


        [Fact]
        public async Task Should_return_correct_companys_when_give_id()
        {
            Company company = new Company("Google");
            var response = await httpClient.PostAsJsonAsync("api/companies", company);
            var responseCompany = await response.Content.ReadFromJsonAsync<Company>();

            var httpResponseMessage = await httpClient.GetAsync($"api/companies/{responseCompany.Id}");
            var creatCompany = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            Assert.Equal(responseCompany.Name,creatCompany.Name);
        }

        [Fact]
        public async Task Should_return_pagesize_from_pageindex_result_give_pagesize_and_pageindex_when_get_pages()
        {
            await ClearDataAsync();
            for (int i = 0; i < 10; i++)
            {
               Company companyGiven = new Company($"Company{i}");
               await httpClient.PostAsync("/api/companies",SerializeObjectToContent(companyGiven));
            }
            int pageIndex = 2;
            int pageSize = 2;

            var response = await httpClient.GetAsync($"api/companies/page?pageSize={pageSize}&pageIndex={pageIndex}");
            var companys = await response.Content.ReadFromJsonAsync<List<Company>>();
            Assert.Equal(pageSize, companys.Count);
            Assert.Equal("Company2", companys[0].Name);

        }
        [Fact]
        public async Task Should_return_update_company_with_status_201_when_update_cpmoany_name_by_id()
        {
            await ClearDataAsync();
            Company companyGiven = new Company("Google");
            var response =  await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var responsecompany = await  response.Content.ReadFromJsonAsync<Company>();
            var updatecompany = new Company("google");
            var updateResponse =  await httpClient.PutAsJsonAsync($"api/companies/{responsecompany.Id}", updatecompany);
            

            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        }
    }
}