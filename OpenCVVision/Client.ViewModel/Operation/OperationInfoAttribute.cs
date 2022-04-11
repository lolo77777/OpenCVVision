namespace Client.ViewModel;

public class OperationInfoAttribute : Attribute
{
    public string Icon { get; private set; }
    public double Id { get; private set; }
    public string Info { get; private set; }

    public OperationInfoAttribute(double id, string info, string icon)
    {
        Info = info;
        Icon = icon;
        Id = id;
    }
}