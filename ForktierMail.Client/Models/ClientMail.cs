using ForktierMail.Shared.Models;

namespace ForktierMail.Client.Models;

public class ClientMail(ForktierMailClient mailClient) : DataMail<ClientFork, ClientPlayer, ClientCharacter, ClientMail>
{
    public ForktierMailClient MailClient = mailClient;
}