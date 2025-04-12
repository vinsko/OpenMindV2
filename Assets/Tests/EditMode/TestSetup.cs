// This program has been developed by students from the bachelor Computer Science at Utrecht University within the Software Project course.
// © Copyright Utrecht University (Department of Information and Computing Sciences)
using System.Reflection;
using NUnit.Framework;

[SetUpFixture]
public class TestSetupEditMode
{
    [OneTimeSetUp]
    public void DisablePopups()
    {
        FieldInfo fieldInfo = typeof(DebugManager).GetField("FullyDisablePopups",
            BindingFlags.NonPublic | BindingFlags.Static);
        fieldInfo.SetValue(null, true);
    }
}