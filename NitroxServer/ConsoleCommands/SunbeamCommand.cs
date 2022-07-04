using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands;

public class SunbeamCommand : Command
{
    private readonly EventTriggerer eventTriggerer;

    // We shouldn't let the server use this command because it needs some stuff to happen client-side like goals
    public SunbeamCommand(EventTriggerer eventTriggerer) : base("sunbeam", Perms.ADMIN, PermsFlag.NO_CONSOLE, "Manage Sunbeam's story state")
    {
        AddParameter(new TypeString("countdown/gunaim/story", true, "Which action to pick from Sunbeam story"));

        this.eventTriggerer = eventTriggerer;
    }

    protected override void Execute(CallArgs args)
    {
        string action = args.Get<string>(0);

        switch (action.ToLower())
        {
            case "countdown":
                eventTriggerer.StartSunbeamCountdown();
                break;
            case "gunaim":
                eventTriggerer.PrecursorGunAim();
                break;
            case "story":
                eventTriggerer.StartSunbeamStory();
                break;
            default:
                // Same message as in the abstract class, in method TryExecute
                SendMessage(args.Sender, $"Error: Invalid Parameters\nUsage: {ToHelpText(false, true)}");
                break;
        }
    }
}
