using ForktierMail.Shared.Models;

namespace ForktierMail.Shared.Interface;

public interface IMailClient
{
    Task<bool> OnMailRecieved(SharedMail mail);
    // Task<bool> OnServerHandshake(ServerHandshakeData handshake);

    Task OnPlayerAdded(SharedPlayer player);
    Task OnCharacterAdded(SharedCharacter character);
    Task OnPlayerRemoved(Guid playerId);
    Task OnCharacterRemoved(int forkId, int characterId);
    Task OnMailUpdated(SharedMail mail);
    Task OnPlayerUpdated(SharedPlayer player);
    Task OnCharacterUpdated(SharedCharacter character);
    Task OnMailRemoved(int mailId);
    Task OnForkRemoved(int forkId);
}