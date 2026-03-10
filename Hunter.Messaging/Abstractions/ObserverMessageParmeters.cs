using Messaging.Abstractions;

namespace CoMessaging.Abstractions;

public class ObserverMessageParmeters
{
    public enum logOp { Add, Remove, None };
    
    public Guid     id            { get; set; }
    public logOp    Operation     { get; set; } = logOp.None;

    public ObserverMessageTypes MsgTypes      { get; set; }     // Type(s) of log info requested
    public List<string> Projects  { get; set; }     // Selected Projects to restrict to; if null = "All"
    public int      UserId        { get; set; }     // if 0, all; else selected UserId msgs
}
