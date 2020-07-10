# Injected Pen Pressure Bug
When using `Windows.Input.Preview.Injection.InputInjector` to inject pen inputs, the actual injected pen pressure will be one quarter of the value set on `InjectedInputPenInfo.Pressure`. So if `InjectedInputPenInfo.Pressure` is set to 1, the APIs will inject a pen input with a pressure of 0.25. If it is set to 0.5, the APIs will inject a pen input with a pressure of 0.125.

Setting `InjectedInputPenInfo.Pressure` to a value greater than 1 will always generate an exception, so you cannot set it to 4 and get an injected input with a pressure of 1.

There are two ways to interact with this repro app after it's launched. The first way is to click or tap the top half of the app with pen, mouse and touch, then the app will inject a pen input with equivalent pressure in the bottom half. The second way is to move the slider to any pressure value you want, then click the "Inject an input" button, which will inject a pen input in the bottom half with pressure equal to the value of the slider. In both cases, the generated input's pressure will be 1/4 of the pressure specified.

In this repro, inputs are injected in the `ProcessTopGridInput` and `manualInputButton_Click` methods in MainPage.xaml.cs.
