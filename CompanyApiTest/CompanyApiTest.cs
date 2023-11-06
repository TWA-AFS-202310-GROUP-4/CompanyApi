using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Linq;
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

        [Theory]
        [InlineData(2,"acompany", "bcomoany")]
        public async Task Should_return_all_companies_when_get_all_companies_given_nothing(int expectedLength,params string[] companyname)
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

    }
}