

public class UiDataModel
{
    private UiDataType type;
    private string value;

    public UiDataModel(UiDataType type, string value)
    {
        this.type = type;
        this.value = value;
    }

    public UiDataType GetUiDataType => type;
    public string GetValue => value;
}
