using System.Collections.Concurrent;

namespace ForktierMail.Shared.Models;

public class DefaultDataPlayer : DataPlayer<DefaultDataFork, DefaultDataPlayer, DefaultDataCharacter, DefaultDataMail>;

public class DataPlayer<F, P, C, M> : SharedPlayer
    where F : DataFork<F, P, C, M>
    where P : DataPlayer<F, P, C, M>
    where C : DataCharacter<F, P, C, M>
    where M : DataMail<F, P, C, M>
{
    /// <summary>
    ///     Dict<forkId, Dict<characterId, ClientCharacter>>
    /// </summary>
    public ConcurrentDictionary<int, ConcurrentDictionary<int, C>> Characters = new();

    /// <summary>
    ///     To avoid confusion; this basically just tell you what fork this player is in lmao
    /// </summary>
    public List<F> Forks = new();

    //TODO: probably format it better or idk fucking From, To dict? idk!

    public List<M> RecievedMails = new();
    public List<M> SentMails = new();
}