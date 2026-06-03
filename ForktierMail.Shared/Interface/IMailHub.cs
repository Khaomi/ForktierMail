using ForktierMail.Shared.Models;

namespace ForktierMail.Shared.Interface;

public interface IMailHub
{
    Task<SharedFork> GetIdentity();
    Task<bool> SendMail(SharedMail mail);
    Task<bool> SendHandshake(ClientHandshakeData handshake);

    Task<bool> AddPlayer(SharedPlayer player);
    Task<bool> AddCharacter(SharedCharacter character);
    Task<bool> RemovePlayer(Guid playerId);
    Task<bool> RemoveCharacter(int characterId);
    Task<bool> UpdatePlayer(SharedPlayer player);
    Task<bool> UpdateCharacter(SharedCharacter character);
    Task<bool> DeleteMail(int mailId);

    Task<ServerHandshakeData?> GetHandshakeData();
}