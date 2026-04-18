// (c) 2025 Infiziert90 (ChatTwo) (modified from ChatCode.cs)
namespace SimpleTriggers.ChatEnums;

public static class ChatTypeExtensions
{
    public static ChatType ParentType(this ChatType value)
    {
        return value switch
        {
            ChatType.Say => ChatType.Say,
            ChatType.GmSay => ChatType.Say,
            ChatType.Shout => ChatType.Shout,
            ChatType.GmShout => ChatType.Shout,
            ChatType.TellOutgoing => ChatType.TellOutgoing,
            ChatType.TellIncoming => ChatType.TellOutgoing,
            ChatType.GmTell => ChatType.TellOutgoing,
            ChatType.Party => ChatType.Party,
            ChatType.CrossParty => ChatType.Party,
            ChatType.GmParty => ChatType.Party,
            ChatType.Linkshell1 => ChatType.Linkshell1,
            ChatType.GmLinkshell1 => ChatType.Linkshell1,
            ChatType.Linkshell2 => ChatType.Linkshell2,
            ChatType.GmLinkshell2 => ChatType.Linkshell2,
            ChatType.Linkshell3 => ChatType.Linkshell3,
            ChatType.GmLinkshell3 => ChatType.Linkshell3,
            ChatType.Linkshell4 => ChatType.Linkshell4,
            ChatType.GmLinkshell4 => ChatType.Linkshell4,
            ChatType.Linkshell5 => ChatType.Linkshell5,
            ChatType.GmLinkshell5 => ChatType.Linkshell5,
            ChatType.Linkshell6 => ChatType.Linkshell6,
            ChatType.GmLinkshell6 => ChatType.Linkshell6,
            ChatType.Linkshell7 => ChatType.Linkshell7,
            ChatType.GmLinkshell7 => ChatType.Linkshell7,
            ChatType.Linkshell8 => ChatType.Linkshell8,
            ChatType.GmLinkshell8 => ChatType.Linkshell8,
            ChatType.FreeCompany => ChatType.FreeCompany,
            ChatType.GmFreeCompany => ChatType.FreeCompany,
            ChatType.NoviceNetwork => ChatType.NoviceNetwork,
            ChatType.GmNoviceNetwork => ChatType.NoviceNetwork,
            ChatType.CustomEmote => ChatType.CustomEmote,
            ChatType.StandardEmote => ChatType.StandardEmote,
            ChatType.Yell => ChatType.Yell,
            ChatType.GmYell => ChatType.Yell,
            ChatType.GainBuff => ChatType.GainBuff,
            ChatType.LoseBuff => ChatType.GainBuff,
            ChatType.GainDebuff => ChatType.GainDebuff,
            ChatType.LoseDebuff => ChatType.GainDebuff,
            ChatType.System => ChatType.System,
            ChatType.Alarm => ChatType.System,
            ChatType.GlamourNotifications => ChatType.System,
            ChatType.RetainerSale => ChatType.System,
            ChatType.PeriodicRecruitmentNotification => ChatType.System,
            ChatType.Sign => ChatType.System,
            ChatType.Orchestrion => ChatType.System,
            ChatType.MessageBook => ChatType.System,
            ChatType.NpcDialogue => ChatType.NpcDialogue,
            ChatType.NpcAnnouncement => ChatType.NpcDialogue,
            ChatType.LootRoll => ChatType.LootRoll,
            ChatType.RandomNumber => ChatType.LootRoll,
            ChatType.FreeCompanyAnnouncement => ChatType.FreeCompanyAnnouncement,
            ChatType.FreeCompanyLoginLogout => ChatType.FreeCompanyAnnouncement,
            ChatType.PvpTeamAnnouncement => ChatType.PvpTeamAnnouncement,
            ChatType.PvpTeamLoginLogout => ChatType.PvpTeamAnnouncement,
            _ => 0,
        };
    }

    public static bool IsBattle(this ChatType value)
    {
        switch (value)
        {
            case ChatType.Damage:
            case ChatType.Miss:
            case ChatType.Action:
            case ChatType.Item:
            case ChatType.Healing:
            case ChatType.GainBuff:
            case ChatType.LoseBuff:
            case ChatType.GainDebuff:
            case ChatType.LoseDebuff:
            case ChatType.BattleSystem:
                return true;
            default:
                return false;
        }
    }

    public static bool IsPlayerChat(this ChatType value)
    {
        switch (value)
        {
            case ChatType.Say:
            case ChatType.Shout:
            case ChatType.Echo:
            case ChatType.TellOutgoing:
            case ChatType.TellIncoming:
            case ChatType.Party:
            case ChatType.CrossParty:
            case ChatType.PvpTeam:
            case ChatType.Linkshell1:
            case ChatType.Linkshell2:
            case ChatType.Linkshell3:
            case ChatType.Linkshell4:
            case ChatType.Linkshell5:
            case ChatType.Linkshell6:
            case ChatType.Linkshell7:
            case ChatType.Linkshell8:
            case ChatType.CrossLinkshell1:
            case ChatType.CrossLinkshell2:
            case ChatType.CrossLinkshell3:
            case ChatType.CrossLinkshell4:
            case ChatType.CrossLinkshell5:
            case ChatType.CrossLinkshell6:
            case ChatType.CrossLinkshell7:
            case ChatType.CrossLinkshell8:
            case ChatType.FreeCompany:
            case ChatType.NoviceNetwork:
            case ChatType.CustomEmote:
            case ChatType.StandardEmote:
            case ChatType.Yell:
                return true;
            default:
                return false;
        }
    }

    public static bool IsAnnouncement(this ChatType value)
    {
        switch (value)
        {
            case ChatType.Urgent:
            case ChatType.Notice:
            case ChatType.GlamourNotifications:
            case ChatType.Alarm:
            case ChatType.Echo:
            case ChatType.System:
            case ChatType.BattleSystem:
            case ChatType.GatheringSystem:
            case ChatType.Error:
            case ChatType.NpcDialogue:
            case ChatType.NpcAnnouncement:
            case ChatType.LootNotice:
            case ChatType.LootRoll:
            case ChatType.Progress:
            case ChatType.Crafting:
            case ChatType.Gathering:
            case ChatType.FreeCompanyAnnouncement:
            case ChatType.FreeCompanyLoginLogout:
            case ChatType.RetainerSale:
            case ChatType.Sign:
            case ChatType.RandomNumber:
            case ChatType.NoviceNetworkSystem:
            case ChatType.Orchestrion:
            case ChatType.PvpTeamAnnouncement:
            case ChatType.PvpTeamLoginLogout:
            case ChatType.MessageBook:
                return true;
            default:
                return false;
        }
    }
}