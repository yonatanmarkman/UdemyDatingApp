using API.Interfaces;

namespace API.Data;

public class MembersDataCache : IMembersDataCache<DateTime>
{
    private static Dictionary<string, DateTime> _memberToMessageCount = new Dictionary<string, DateTime>();

    public MembersDataCache()
    {
        
    }

    public DateTime GetMemberData(string recipientId)
    {
        if (_memberToMessageCount.ContainsKey(recipientId))
            return _memberToMessageCount[recipientId];
        
        return DateTime.MinValue;
    }
    
    public bool SetMemberData(string recipientId, DateTime memberData)
    {
        DateTime currentData = GetMemberData(recipientId);

        if (memberData > currentData)
        {
            _memberToMessageCount[recipientId] = memberData;
            return true;
        }

        return false;
    }

}
