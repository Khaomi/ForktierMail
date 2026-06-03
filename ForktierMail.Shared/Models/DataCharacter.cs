namespace ForktierMail.Shared.Models;

public class
    DefaultDataCharacter : DataCharacter<DefaultDataFork, DefaultDataPlayer, DefaultDataCharacter, DefaultDataMail>;

public class DataCharacter<F, P, C, M>
    : SharedCharacter
    where F : DataFork<F, P, C, M>
    where P : DataPlayer<F, P, C, M>
    where C : DataCharacter<F, P, C, M>
    where M : DataMail<F, P, C, M>
{
    public required F Fork;
    public required P Player;

    //TODO: probably format it better or idk fucking From, To dict? idk!

    public List<M> RecievedMails = new();
    public List<M> SentMails = new();
}