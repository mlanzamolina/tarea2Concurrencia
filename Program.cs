using System;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using System.Text.Json;

namespace tarea_2; // Note: actual namespace depends on the project name.

internal class Program
{
    static async Task Main(string[] args)
    {
       List<Posts> postsList;
       List<Data> dataList;
        dataList = new List<Data>();
        var client = new RestClient("https://jsonplaceholder.typicode.com");
        var request = new RestRequest("posts", Method.Get);
        var response = await client.ExecuteGetAsync(request);
        var content = response.Content;
        postsList = JsonConvert.DeserializeObject<List<Posts>>(content);
        foreach (var post in postsList)
        {
            var requestComments = new RestRequest($"comments?postId={post.id}", Method.Get);
            var responseComments = await client.ExecuteGetAsync(requestComments);
            var contentComments = responseComments.Content;
            var commentsList = JsonConvert.DeserializeObject<List<Comments>>(contentComments);
           
            dataList.Add(new Data { id = post.id, comments = commentsList });     
        }
        foreach (var data in dataList)
        {
            Console.WriteLine($"Post {data.id} has {data.comments.Count} comments");
        }

        //Utilizando el API:https://jsonplaceholder.typicode.com/posts/{i} donde i es un entero de 1 a 100,
        // obtener de manera asincrona todos los posts y luego por cada post obtener todos los comentarios 
        //de cada post de manera asincrona utilizando: https://jsonplaceholder.typicode.com/posts/{i}/comments, 
        //donde i es el id del post (de 1 a 100). Luego de manera paralela escribir un archivo de texto para cada post junto con sus comentarios. 
        //Al final existiran 100 archivos cada uno con los datos y comentarios del post.
       
       // Test().Wait();
        Console.WriteLine("\nEnded wwait for the end of the program");


    }
    public class Posts
    {
        public int? userId { get; set; }
        public int? id { get; set; }
        public string? title { get; set; }
        public string? body { get; set; }
    }
    public class Comments
    {
        public int? postId { get; set; }
        public int? id { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? body { get; set; }
    }
    public class Data
    {
        public int? id { get; set; }
        public List<Comments> comments { get; set; }
    }
}