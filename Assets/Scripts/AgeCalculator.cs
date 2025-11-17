using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[Serializable]
public class AgeData
{
    public int Years;
    public int Months;
    public int Days;

    public DateTime? BirthDay;

    private int years;
    private int months;
    private int days;

    private int previousYears;
    private int previousMonths;
    private int previousDays;

    private float current = 0f;
    private float duration = 0.5f;

    public void SetBirthday(DateTime birthDay)
    {
        current = 0f;

        previousDays = Days;
        previousMonths = Months;
        previousYears = Years;

        BirthDay = birthDay;

        TimeSpan timeSpan = DateTime.Now - BirthDay.Value;

        years = (int)(timeSpan.Days / 365.25);
        months = (int)((timeSpan.Days % 365.25) / 30.44);
        days = (int)((timeSpan.Days % 365.25) % 30.44);
    }

    public void UpdateAge(float deltaTime)
    {
        if (BirthDay == null)
        {
            return;
        }

        float progress = current / duration;
        current += deltaTime;

        Years = Mathf.RoundToInt(Mathf.Lerp(previousYears, years, progress));
        Months = Mathf.RoundToInt(Mathf.Lerp(previousMonths, months, progress));
        Days = Mathf.RoundToInt(Mathf.Lerp(previousDays, days, progress));
    }
}

public class AgeCalculator : MonoBehaviour
{
    [SerializeField] private InputFieldData dayInputField;
    [SerializeField] private InputFieldData monthInputField;
    [SerializeField] private InputFieldData yearInputField;

    private InputFieldData[] inputFieldData => new[] { dayInputField, monthInputField, yearInputField };

    [SerializeField] private AgeData data;

    private UIDocument uiDocument;

    private VisualElement stats;
    private VisualElement view;
    private VisualElement buttonPosition;

    private Button button;

    protected void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    protected void OnEnable()
    {
        dayInputField.Validate = value => IsInBetween(value, DateTime.MinValue.Day, DateTime.MaxValue.Day);
        monthInputField.Validate = value => IsInBetween(value, DateTime.MinValue.Month, DateTime.MaxValue.Month);
        yearInputField.Validate = value => IsInBetween(value, DateTime.MinValue.Year, DateTime.MaxValue.Year);

        foreach (var item in inputFieldData)
        {
            VisualElement vs = uiDocument.rootVisualElement.Q<VisualElement>(item.Id).Children().First();
            IntegerField integerField = vs.Q<IntegerField>();
            TextField textField = vs.Q<TextField>();
            textField.RegisterValueChangedCallback((evt) => OnTextChangedEvent(textField, evt));
            vs.dataSource = item;
        }

        buttonPosition = uiDocument.rootVisualElement.Q<VisualElement>("ButtonPosition");

        view = uiDocument.rootVisualElement.Q<VisualElement>("view");

        stats = uiDocument.rootVisualElement.Q<VisualElement>("StatsContainer");
        stats.dataSource = data;

        button = uiDocument.rootVisualElement.Q<Button>("ButtonSubmit");
        button.clicked += OnCalculateButtonClicked;
    }

    protected void Update()
    {
        data.UpdateAge(Time.deltaTime);

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            button.Focus();
            OnCalculateButtonClicked();
        }

        if (Screen.width < Screen.height)
        {
            view.style.width = new StyleLength(Length.Percent(98));
            buttonPosition.style.justifyContent = Justify.Center;
        }
        else
        {
            view.style.width = new StyleLength(Length.Percent(50));
            buttonPosition.style.justifyContent = Justify.FlexEnd;
        }
    }

    protected void OnDisable()
    {
        button.clicked -= OnCalculateButtonClicked;
    }

    private void OnTextChangedEvent(TextField textField, ChangeEvent<string> evt)
    {
        if (evt.newValue.Length == 0)
        {
            return;
        }

        if(!int.TryParse(evt.newValue, out int _))
        {
            textField.SetValueWithoutNotify(evt.previousValue);
        }
    }

    private void OnCalculateButtonClicked()
    {
        bool error = false;

        foreach (var item in inputFieldData)
        {
            item.CurrentErrorMessage = string.Empty;

            int value = 0;

            if (item.Value.Length == 0)
            {
                error = true;
                item.CurrentErrorMessage = "This field is required.";
                continue;
            }

            if (!int.TryParse(item.Value, out value))
            {
                error = true;
                item.CurrentErrorMessage = "Not an number.";
                continue;
            }

            if (!item.Validate(value))
            {
                error = true;
                item.CurrentErrorMessage = item.ErrorMessage;
                continue;
            }
        }

        uiDocument.rootVisualElement.EnableInClassList("error", error);

        if (error)
        {
            return;
        }

        try
        {
            DateTime birthDay = new DateTime(inputFieldData[2].ParseValue(), inputFieldData[1].ParseValue(), inputFieldData[0].ParseValue());
            data.SetBirthday(birthDay);
        }
        catch (ArgumentOutOfRangeException)
        {
            Debug.LogError("Invalid date provided.");
            inputFieldData[0].CurrentErrorMessage = "Must be a valid date";
            inputFieldData[1].CurrentErrorMessage = string.Empty;
            inputFieldData[2].CurrentErrorMessage = string.Empty;
            uiDocument.rootVisualElement.EnableInClassList("error", true);
        }
    }

    private bool IsInBetween(int value, int min, int max)
    {
        return value >= min && value <= max;
    }
}

[Serializable]
public class InputFieldData
{
    public int ParseValue() => int.Parse(Value);

    public string Id;

    public string Title;

    public string Placeholder;

    public string Value;

    public int MaxLength;

    public string CurrentErrorMessage;

    public string ErrorMessage;

    public Func<int, bool> Validate;
}