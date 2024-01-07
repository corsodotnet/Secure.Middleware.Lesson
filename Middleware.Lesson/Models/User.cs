namespace Middleware.Lesson.Models
{
    public class User
    {
        public int Id { get; set; }
        // Informazioni generali dell'utente
        public string Username { get; set; } // Username o nickname
        public string Email { get; set; }    // Email dell'utente

        // Campi per l'autenticazione basata su email e password
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        // Campi per l'autenticazione OAuth (es. Google)
        public string GoogleId { get; set; }  // ID univoco fornito da Google, null per gli utenti non Google

        // Altri campi rilevanti per la tua applicazione come data di registrazione, ruoli, ecc.
        // public DateTime DateRegistered { get; set; }
        // public string Role { get; set; }
    }



    public class UserDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

}
