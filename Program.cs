using RestSharp;
using Newtonsoft.Json;

//Utilizando el API:https://jsonplaceholder.typicode.com/posts/{i} donde i es un entero de 1 a 100,
// obtener de manera asincrona todos los posts y luego por cada post obtener todos los comentarios 
//de cada post de manera asincrona utilizando: https://jsonplaceholder.typicode.com/posts/{i}/comments, 
//donde i es el id del post (de 1 a 100). Luego de manera paralela escribir un archivo de texto para cada post junto con sus comentarios. 
//Al final existiran 100 archivos cada uno con los datos y comentarios del post.

namespace tarea_2;
internal partial class Program
{
    private static async Task<List<Posts>> GetPosts(List<Posts> postsList)
    {

        var client = new RestClient("https://jsonplaceholder.typicode.com");
        var request = new RestRequest("posts", Method.Get);
        var response = await client.ExecuteGetAsync(request);
        var content = response.Content;
        postsList = JsonConvert.DeserializeObject<List<Posts>>(content);
        System.Console.WriteLine($"Getting Posts in thread {Thread.CurrentThread.ManagedThreadId}");
        return postsList;
    }
    private static async Task<List<Data>> GetComments(List<Posts> postsList, List<Data> dataList)
    {
        var client = new RestClient("https://jsonplaceholder.typicode.com");
        Parallel.ForEach(postsList, async p =>
                   {
                       var requestComments = new RestRequest($"comments?postId={p.id}", Method.Get);
                       var responseComments = await client.ExecuteGetAsync(requestComments);
                       var contentComments = responseComments.Content;
                       var commentsList = JsonConvert.DeserializeObject<List<Comments>>(contentComments);
                       dataList.Add(new Data { post = p, comments = commentsList });
                       System.Console.WriteLine($"Getting Comments in thread {Thread.CurrentThread.ManagedThreadId}");
                       //i++;
                   });
        return dataList;
    }
    static async Task Main(string[] args)
    {
        string dataPath = @"data";
        try
        {
            // Determine whether the directory exists.
            if (Directory.Exists(dataPath))
            {
                Console.WriteLine("That path exists already. Continuing...");
            }
            else
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(dataPath);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(dataPath));
            }

        }
        catch (Exception e)
        {
            Console.WriteLine("The process failed: {0}", e.ToString());
        }
        // Create a new list of posts and data that contains the posts and comments.
        List<Posts> postsList = new List<Posts>();
        List<Data> dataList = new List<Data>();

        // Get the posts from the API.
        Task loadPosts = Task.Run(async () =>
        {
            postsList = await GetPosts(postsList);
        });
        loadPosts.Wait();
        // Get the comments of post from the API.
        int i = 1;
        foreach (var data in postsList)
        {
            Console.WriteLine($"Post [{i}] {data.id} ");
            i++;
        }
        i = 0;
        Task loadComments = Task.Run(async () =>
        {
            dataList = await GetComments(postsList, dataList);
        });
        loadComments.Wait();
         i = 1;
         
        Console.WriteLine($"Done total {dataList.Count}");
       


        Task createFiles = Task.Run(() =>
       {
           Parallel.ForEach(postsList, data =>
                      {
                          string path = $@"data\{data.id}.txt";
                          using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path)) ;
                      });
       });
        createFiles.Wait();

        // Write the data to a file.
        // Task writeToFiles = Task.Run(async () =>
        // {
        //     Parallel.ForEach(dataList, async data =>
        //                {
        //                    string path = $@"data\{data.post.title}.txt";
        //                    using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path));
        //                    Parallel.ForEach(data.comments, async comment =>
        //                         {
        //                             System.Console.WriteLine($"Comment: {comment.name} in thread {Thread.CurrentThread.ManagedThreadId}");
        //                             await File.AppendAllTextAsync(path, $"\nName: {comment.name}\nComment: {comment.body}\n");
        //                         });

        //                   await File.AppendAllTextAsync(path, "\n)\n");
        //                });
        // });
        // writeToFiles.Wait();

        Console.WriteLine("\nRun exitoso");
    }

}