using System;

public class ContextMenuItem
{
    public string Name => this.name;

    public Action Action => this.action;

    public ContextMenuItem(string name, Action action)
    {
        this.name = name;
        this.action = action;
    }

    string name = "";
    Action action = null;
}
