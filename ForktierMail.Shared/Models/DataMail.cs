namespace ForktierMail.Shared.Models;

public class DefaultDataMail : DataMail<DefaultDataFork, DefaultDataPlayer, DefaultDataCharacter, DefaultDataMail>;

public class DataMail<F, P, C, M> : SharedMail
    where F : DataFork<F, P, C, M>
    where P : DataPlayer<F, P, C, M>
    where C : DataCharacter<F, P, C, M>
    where M : DataMail<F, P, C, M>
{
    public required C Recipient;
    public required F RecipientFork;

    public required C Sender;
    public required F SenderFork;
}