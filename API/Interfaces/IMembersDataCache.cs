namespace API.Interfaces;

public interface IMembersDataCache<T>
{
    T GetMemberData(string recipientId);

    bool SetMemberData(string recipientId, T memberData);
}