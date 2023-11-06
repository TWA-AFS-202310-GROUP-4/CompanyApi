using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
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
        public async Task Should_return_all_company_list_when_get_all_company_given_create_one_companies()
        {
            //Given
            await ClearDataAsync();
            Company companyGiven1 = new Company("BlueSky Digital Media");
            await httpClient.PostAsJsonAsync("/api/companies", companyGiven1);
            //When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies");

            //Then
            List<Company> expectedResult = new List<Company>();
            expectedResult.Add(companyGiven1);
            string companiesReturned = await httpResponseMessage.Content.ReadAsStringAsync();
            List<Company>? companyReturned = JsonConvert.DeserializeObject<List<Company>>(companiesReturned);

            Assert.Equal(expectedResult[0].Name, companyReturned[0].Name);

        }
        //public async Task Should
        [Fact]
        public async Task Should_return_the_company_when_get_by_id_given_the_company_existed()
        {
            //Given
            await ClearDataAsync();
            Company companyGiven1 = new Company("BlueSky Digital Media");
            HttpResponseMessage httpPostReturnedMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven1);
            string companiesReturned = await httpPostReturnedMessage.Content.ReadAsStringAsync();
            Company? company = JsonConvert.DeserializeObject<Company>(companiesReturned);

            //When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies/" + company.Id);

            //Then
            string companiesGetReturned = await httpResponseMessage.Content.ReadAsStringAsync();
            Company? companyGetReturned = JsonConvert.DeserializeObject<Company>(companiesGetReturned);
            Assert.Equal(company.Name, companyGetReturned.Name);
        }

        public async Task Should_return_not_found_when_get_by_id_given_the_company_not_existed()
        {
            //Given
            await ClearDataAsync();

            //When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies/" + "123");

            //Then
            Assert.Equal(httpResponseMessage.StatusCode, HttpStatusCode.NotFound);
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