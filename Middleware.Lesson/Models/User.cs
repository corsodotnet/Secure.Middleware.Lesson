namespace Middleware.Lesson.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

    }


    public class UserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

}
