using ForktierMail.Shared.Models;

namespace ForktierMail.Client.Models;

public class ClientPlayer(ForktierMailClient mailClient)
    : DataPlayer<ClientFork, ClientPlayer, ClientCharacter, ClientMail>
{
    public ForktierMailClient MailClient = mailClient;
}