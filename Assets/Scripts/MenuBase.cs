using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBase
{
    private float menuXpos = 20;
    private float menuYpos = 20;

    #region DO NOT TOUCH
    public float menuItemWidth = 160f;
    private float menuItemHeight = 30f;
    public float menuHeight = 0f;
    private float whiteSpace = 5f;
    #endregion

    private string menuTitle = "menu";
    public string title 
    { 
        get { return menuTitle; } 
        set { menuTitle = "<size=15><b>" + value +"</b></size>"; } 
    }

    public MenuBase()
    {
        //infoPanel.xPos = Screen.width - 240;
        infoPanel.xPos = menuXpos + menuItemWidth + 15;
        infoPanel.yPos = 20;
        infoPanel.height = 275;//260;//250;//190;//130; //öka med typ 20 per rad
        infoPanel.width = 210;
    }

    Rect CreateRect(float _menuItemHeight = 0f, float adjustPosition = 0f, float adjustWidth = 0f)
    {
        if (_menuItemHeight < 0.1f)
            _menuItemHeight = menuItemHeight;
        Rect rect = new Rect((adjustWidth / (-2)) + 5f + menuXpos, menuYpos + menuHeight + adjustPosition, menuItemWidth + adjustWidth, _menuItemHeight);
        menuHeight += _menuItemHeight + whiteSpace;
        return rect;
    }

    public void AddButton(string label, ref bool bPressed, bool subMenu = false)
    {
        if (subMenu)
        {
            Color temp = GUI.backgroundColor;
            GUI.backgroundColor = Color.blue;
            if (GUI.Button(CreateRect(), label))
                bPressed = !bPressed;
            GUI.backgroundColor = temp;
        }
        else
            if (GUI.Button(CreateRect(), label))
            bPressed = !bPressed;
    }

    public bool AddBoolButton(string label)
    {
        if (GUI.Button(CreateRect(), label))
            return true;
        return false;

    }

    public void AddBreak()
    {
        /*
        Color temp = GUI.color;
        GUI.color = Color.red;
        GUI.backgroundColor = Color.red;
        GUI.Box(CreateRect(_menuItemHeight: 1f), "");
        GUI.color = temp;
        */
    }
    public void AddLabel(string text)
    {
        GUI.Box(CreateRect(_menuItemHeight: 24f), text);
    }

    public void Add2RowLabel(string text)
    {
        GUI.Box(CreateRect(_menuItemHeight: 36f), text);
    }

    public void AddSwitcher(string labelText, ref string valueText, ref int value, int minValue, int maxValue)
    {
        Rect mainBox = new Rect(5f + menuXpos, menuYpos + menuHeight + 0, 160f, menuItemHeight+19f);
        GUI.Box(mainBox, labelText);
        menuHeight += 25f;

        Rect center = new Rect(5f + menuXpos, menuYpos + menuHeight + 0, 160f, menuItemHeight-6f);
        GUI.Box(center, valueText);

        Rect left = new Rect(5f + menuXpos, menuYpos + menuHeight + 0,  20f, menuItemHeight - 6f);
        if (GUI.Button(left, "<b><</b>"))
            value = (value > minValue) ? value-1 : maxValue;

        Rect right = new Rect(5f + menuXpos + 140f, menuYpos + menuHeight + 0, 20f, menuItemHeight - 6f);
        if (GUI.Button(right, "<b>></b>"))
            value = (value < maxValue) ? value + 1 : minValue;

        menuHeight += menuItemHeight + whiteSpace -5f;
    }
    public void AddButtonWithLabel(string labelText, string buttonText)
    {
        GUI.Box(CreateRect(_menuItemHeight: (menuItemHeight * 3) - 17f), labelText);
        //textFieldText = GUI.TextField(CreateRect(_menuItemHeight: menuItemHeight - 10f, adjustPosition: -(menuItemHeight + 29f), adjustWidth: -10f), textFieldText);
        menuHeight -= 23f + menuItemHeight + 7f;
        bool buttonPressed = false;
        AddButton(buttonText, ref buttonPressed);
    }

    public void AddTextFieldWithLabel(string labelText, ref string textFieldText)
    {
        GUI.Box(CreateRect(_menuItemHeight: (menuItemHeight * 2) - 14f), labelText);
        textFieldText = GUI.TextField(CreateRect(_menuItemHeight: menuItemHeight - 10f, adjustPosition: -31f, adjustWidth: -10f), textFieldText);

        menuHeight -= 23f;
    }
    public void AddTextFieldWithLabel(string labelText, ref float textFieldint)
    {
        string textFieldText = textFieldint.ToString();
        GUI.Box(CreateRect(_menuItemHeight: (menuItemHeight * 2) - 14f), labelText);
        textFieldText = GUI.TextField(CreateRect(_menuItemHeight: menuItemHeight - 10f, adjustPosition: -31f, adjustWidth: -10f), textFieldText);
        try { textFieldint = float.Parse(textFieldText); }
        catch { textFieldint = 0; }

        menuHeight -= 23f;
    }

    public void AddTextFieldWithLabel(string labelText, ref int textFieldint)
    {
        string textFieldText = textFieldint.ToString();
        GUI.Box(CreateRect(_menuItemHeight: (menuItemHeight * 2) - 14f), labelText);
        textFieldText = GUI.TextField(CreateRect(_menuItemHeight: menuItemHeight - 10f, adjustPosition: -31f, adjustWidth: -10f), textFieldText);
        try { textFieldint = int.Parse(textFieldText); }
        catch { textFieldint = 0; }

        menuHeight -= 23f;
    }

    public void AddTextFieldWithLabelAndButton(string labelText, ref string textFieldText, string buttonText, ref bool buttonPressed)
    {
        GUI.Box(CreateRect(_menuItemHeight: (menuItemHeight * 3) - 17f), labelText);
        textFieldText = GUI.TextField(CreateRect(_menuItemHeight: menuItemHeight - 10f, adjustPosition: -(menuItemHeight + 29f), adjustWidth: -10f), textFieldText);
        menuHeight -= 23f + menuItemHeight + 7f;
        AddButton(buttonText, ref buttonPressed);

    }

    public bool AddTextFieldWithLabelAndBoolButton(string labelText, ref string textFieldText, string buttonText)
    {
        GUI.Box(CreateRect(_menuItemHeight: (menuItemHeight * 3) - 17f), labelText);
        textFieldText = GUI.TextField(CreateRect(_menuItemHeight: menuItemHeight - 10f, adjustPosition: -(menuItemHeight + 29f), adjustWidth: -10f), textFieldText);
        menuHeight -= 23f + menuItemHeight + 7f;
        return AddBoolButton(buttonText);

    }

    public void AddTextFieldWithLabelAndToggle(string labelText, ref int textFieldint, string buttonText, ref bool buttonPressed)
    {
        string textFieldText = textFieldint.ToString();
        GUI.Box(CreateRect(_menuItemHeight: (menuItemHeight * 3) - 17f), labelText);
        textFieldText = GUI.TextField(CreateRect(_menuItemHeight: menuItemHeight - 10f, adjustPosition: -(menuItemHeight + 29f), adjustWidth: -10f), textFieldText);
        menuHeight -= 23f + menuItemHeight + 7f;
        AddToggle(buttonText, ref buttonPressed);
        try { textFieldint = int.Parse(textFieldText); }
        catch { textFieldint = 0; }

    }

    public void AddSliderWithLabel(string labelText, ref float value, float min, float max, int decimals = -1)
    {
        GUI.Box(CreateRect(_menuItemHeight: (menuItemHeight * 2) - 9f), labelText + "\n\n" + value.ToString());
        value = GUI.HorizontalSlider(CreateRect(_menuItemHeight: menuItemHeight - 10f, adjustPosition: -35.5f, adjustWidth: -10f), value, min, max);
        if (decimals > -1)
            value = (float)System.Math.Round(value, decimals);
        menuHeight -= 24f;
    }

    public void AddSliderWithLabel(string labelText, ref int _value, int _min, int _max)
    {
        float value = _value, min = _min, max = _max;
        GUI.Box(CreateRect(_menuItemHeight: (menuItemHeight * 2) - 9f), labelText + "\n\n" + value.ToString());
        value = GUI.HorizontalSlider(CreateRect(_menuItemHeight: menuItemHeight - 10f, adjustPosition: -35.5f, adjustWidth: -10f), value, min, max);
        menuHeight -= 24f;
        _value = (int)value;
    }

    public void AddToggle(string label, ref bool value)
    {
        if (value)
            label += " <color=green>ON</color>";
        else
            label += " <color=red>OFF</color>";

        if (GUI.Button(CreateRect(), label))
            value = !value;
    }

    public void AddToggle(string label, ref bool value, ref bool buttonValue)
    {
        if (buttonValue)
            label += " <color=green>ON</color>";
        else
            label += " <color=red>OFF</color>";

        if (GUI.Button(CreateRect(), label))
            value = !value;
    }

    public void AddGameObjectToggle(string buttonText, ref GameObject gameObject, ref bool buttonValue)
    {
        if (gameObject != null)
        {
            bool buttonPressed = false;
            AddToggle(buttonText, ref buttonPressed, ref buttonValue);
            if (buttonPressed)
            {
                buttonValue = !buttonValue;
                gameObject.SetActive(buttonValue);
            }
        }
    }
    
    public void DrawMenuBackground(float subPos = 0)
    {
        GUI.Box(new Rect(20, menuYpos + subPos, menuItemWidth+10, menuYpos + menuHeight), "<size=15><b>" + title + "</b></size>");
        menuHeight = 30f;
    }
    public sMenu infoPanel;
    public struct sMenu
    {
        public float xPos;
        public float yPos;
        public float height;
        public float width;
    }
    public void DrawMenuBackground(sMenu menu, string text)
    {
        GUI.Box(new Rect(menu.xPos, menu.yPos, menu.width, menu.yPos + menu.height), text);
    }
}
