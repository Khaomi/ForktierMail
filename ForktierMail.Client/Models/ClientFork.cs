using ForktierMail.Shared.Models;

namespace ForktierMail.Client.Models;

public class ClientFork(ForktierMailClient mailClient) : DataFork<ClientFork, ClientPlayer, ClientCharacter, ClientMail>
{
    public ForktierMailClient MailClient = mailClient;
}