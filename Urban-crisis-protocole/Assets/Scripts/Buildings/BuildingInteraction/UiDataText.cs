using UnityEngine;

public class UiDataText
{
    private UiDataType type;
    private string value;

    public UiDataText(UiDataType type, string value)
    {
        this.type = type;
        this.value = value;
    }

    public UiDataType GetUiDataType => type;
    public string GetValue => value;
}
