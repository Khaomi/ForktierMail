using System.Collections.Concurrent;

namespace ForktierMail.Shared.Models;

public class DefaultDataFork : DataFork<DefaultDataFork, DefaultDataPlayer, DefaultDataCharacter, DefaultDataMail>;

public class DataFork<F, P, C, M>
    : SharedFork
    where F : DataFork<F, P, C, M>
    where P : DataPlayer<F, P, C, M>
    where C : DataCharacter<F, P, C, M>
    where M : DataMail<F, P, C, M>
{
    /// <summary>
    ///     CharacterId, Character
    /// </summary>
    public ConcurrentDictionary<int, C> Characters = new();

    /// <summary>
    ///     PlayerId, Player
    /// </summary>
    public ConcurrentDictionary<int, P> Players = new();
}