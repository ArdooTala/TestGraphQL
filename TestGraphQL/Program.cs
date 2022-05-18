using System;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using GraphQL.Client;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace TestGraphQL
{
    class RefedObject
    {
        public String referencedObject;
    }

    class Commits
    {
        public List<RefedObject> items;
    }

    class queryResult
    {
        public String id;
        public String name;
        public Commits commits;
    }

    class MainClass
    {
        public static async Task Main()
        {
            //Console.WriteLine("Hello World!");

            //var client = new RestClient(@"http://bettercncfactory.iaac.net/graphql");
            
            //var request = new RestRequest();
            //request.Method = Method.Post;
            //request.Timeout = -1;
            //request.AddHeader("Authorization", "Bearer 48a37d302a44d8451a8cc32e14f82629769100b1c1");
            //request.AddHeader("Content-Type", "application/json");
            //request.AddStringBody("{\"query\":\"query{streams{totalCount}}\"}", DataFormat.Json);

            //RestResponse response = await client.ExecuteAsync(request);

            //Console.WriteLine(response.Content);

            var gqlClient = new GraphQLHttpClient(
                @"http://bettercncfactory.iaac.net/graphql",
                new NewtonsoftJsonSerializer());

            gqlClient.HttpClient.DefaultRequestHeaders.Add(
                "Authorization",
                "Bearer 48a37d302a44d8451a8cc32e14f82629769100b1c1");

            var commitsReq = new GraphQLHttpRequest
            {
                Query = @"query allCommits {
                            streams{
                                items{
                                    id
                                    name
                                    commits (limit: 1) {
                                        items{
                                            referencedObject
                                        }
                                    }
                                }
                            }
                        }"
            };

            var gqlResponse = await gqlClient.SendQueryAsync<dynamic>(commitsReq);

            //Console.WriteLine("Commits raw response:");
            //Console.WriteLine(gqlResponse.Data);

            JObject queryResult = JObject.Parse(gqlResponse.Data.ToString());
            
            // get JSON result objects into a list
            IList<JToken> results = queryResult["streams"]["items"].Children().ToList();

            // serialize JSON results into .NET objects
            IList<queryResult> searchResults = new List<queryResult>();
            foreach (JToken result in results)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                queryResult searchResult = result.ToObject<queryResult>();
                searchResults.Add(searchResult);
                
                //if (searchResult.commits.items.Count > 0)
                //{
                //    Console.Write(searchResult.id);
                //    Console.Write(searchResult.name);
                //    Console.Write("    --->    ");
                //    Console.Write(searchResult.commits.items[0].referencedObject);
                //}
                //Console.WriteLine();
            }

            //var streamsReq = new GraphQLHttpRequest
            //{
            //    Query = @"query {
            //                stream(id: ""6bfa876e9a"") {
            //                    object(id: ""86764ac1ac347f4a7d53ca4374aec866"") {
            //                        id
            //                        totalChildrenCount
            //                        children(depth: 2) {
            //                            objects {
            //                                id
            //                                data
            //                            }
            //                        }
            //                    }
            //                }
            //            }"
            //};

            foreach (var searchResult in searchResults)
            {
                if (searchResult.commits.items.Count <= 0) continue;
                Console.Write(searchResult.id);
                Console.Write("    ");
                Console.Write(searchResult.name);
                Console.Write("    --->    ");
                Console.Write(searchResult.commits.items[0].referencedObject);
                Console.WriteLine();

                var streamsReq = new GraphQLHttpRequest
                {
                    Query = $@"query {{
                            stream(id: ""{searchResult.id}"") {{
                                object(id: ""{searchResult.commits.items[0].referencedObject}"") {{
                                    children(depth: 2) {{
                                        objects {{
                                            id
                                        }}
                                    }}
                                }}
                            }}
                        }}"
                };

                Console.WriteLine(streamsReq.Query);

                gqlResponse = await gqlClient.SendQueryAsync<dynamic>(streamsReq);

                Console.WriteLine("Stream raw response:");
                Console.WriteLine(gqlResponse.Data);
            }

             
            //gqlResponse = await gqlClient.SendQueryAsync<dynamic>(streamsReq);

            //Console.WriteLine("Stream raw response:");
            //Console.WriteLine(gqlResponse.Data);
        }
    }
}

//"query{streams{totalCount}}"
// Bearer 48a37d302a44d8451a8cc32e14f82629769100b1c1
