using ForktierMail.Shared.Models;

namespace ForktierMail.Client.Models;

public class ClientCharacter(ForktierMailClient mailClient)
    : DataCharacter<ClientFork, ClientPlayer, ClientCharacter, ClientMail>
{
    public ForktierMailClient MailClient = mailClient;
}