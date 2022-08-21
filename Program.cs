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
    // private static List<Data> GetComments(List<Posts> postsList, List<Data> dataList)
    // {
    //     foreach (var postItem in postsList)
    //     {
    //         var client = new RestClient("https://jsonplaceholder.typicode.com");
    //         var requestComments = new RestRequest($"comments?postId={postItem.id}", Method.Get);
    //         var responseComments = await client.ExecuteGetAsync(requestComments);
    //         var contentComments = responseComments.Content;
    //         var commentsList = JsonConvert.DeserializeObject<List<Comments>>(contentComments);
    //         dataList.Add(new Data { post = postItem, comments = commentsList });
    //     }
    //     return dataList;
    // }
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
        Task loadComments = Task.Run(() =>
        {
            var client = new RestClient("https://jsonplaceholder.typicode.com");
            Parallel.For(0, postsList.Count,
                        index =>
                       {
                           var requestComments = new RestRequest($"comments?postId={postsList[index].id}", Method.Get);
                           var responseComments = client.Get(requestComments);
                           var contentComments = responseComments.Content;
                           var commentsList = JsonConvert.DeserializeObject<List<Comments>>(contentComments);
                           dataList.Add(new Data { post = postsList[index], comments = commentsList });
                           System.Console.WriteLine($"Getting Comments in thread {Thread.CurrentThread.ManagedThreadId}");
                       });
        });
        loadComments.Wait();

        foreach (var data in dataList)
        {
            Console.WriteLine($"Post {data.post.id} has {data.comments.Count} comments");
        }

        // Write the data to a file.
        Task t = Task.Run(async () =>
        {
            foreach (var item in dataList)
            {
                string path = $@"data\{item.post.title}.txt";
                using (StreamWriter sw = (File.Exists(path)) ? File.AppendText(path) : File.CreateText(path)) ;
                await File.WriteAllTextAsync(path, "Starting process\n");
                System.Console.WriteLine($"Processing PostID: {item.post.id} in thread{Thread.CurrentThread.ManagedThreadId}\nTitle: {item.post.title}");
                await File.AppendAllTextAsync(path, $"(\nPost id: {item.post.id} in thread {Thread.CurrentThread.ManagedThreadId}\n");
                foreach (var comment in item.comments)
                {
                    System.Console.WriteLine($"Comment: {comment.name} in thread {Thread.CurrentThread.ManagedThreadId}");
                    await File.AppendAllTextAsync(path, $"\nName: {comment.name}\nComment: {comment.body}\n");
                }
                await File.AppendAllTextAsync(path, "\n)\n");
            }
        });
        t.Wait();
        Console.WriteLine("\nRun exitoso");
    }

}