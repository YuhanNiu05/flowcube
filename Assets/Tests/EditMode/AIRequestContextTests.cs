using NUnit.Framework;
using Flowcube.AI;

namespace Flowcube.Tests
{
    /// <summary>
    /// Edit-mode tests for AIRequestContext helper methods.
    /// These are pure C# and require no Unity runtime.
    /// </summary>
    public class AIRequestContextTests
    {
        [Test]
        public void TimeOfDayLabel_MidnightHour_ReturnsDeepNight()
        {
            var ctx = new AIRequestContext { currentHour = 0 };
            Assert.AreEqual("深夜", ctx.TimeOfDayLabel());
        }

        [Test]
        public void TimeOfDayLabel_Hour23_ReturnsDeepNight()
        {
            var ctx = new AIRequestContext { currentHour = 23 };
            Assert.AreEqual("深夜", ctx.TimeOfDayLabel());
        }

        [Test]
        public void TimeOfDayLabel_Hour7_ReturnsMorning()
        {
            var ctx = new AIRequestContext { currentHour = 7 };
            Assert.AreEqual("清晨", ctx.TimeOfDayLabel());
        }

        [Test]
        public void TimeOfDayLabel_Hour10_ReturnsMorningSession()
        {
            var ctx = new AIRequestContext { currentHour = 10 };
            Assert.AreEqual("上午", ctx.TimeOfDayLabel());
        }

        [Test]
        public void TimeOfDayLabel_Hour13_ReturnsNoon()
        {
            var ctx = new AIRequestContext { currentHour = 13 };
            Assert.AreEqual("正午", ctx.TimeOfDayLabel());
        }

        [Test]
        public void TimeOfDayLabel_Hour16_ReturnsAfternoon()
        {
            var ctx = new AIRequestContext { currentHour = 16 };
            Assert.AreEqual("下午", ctx.TimeOfDayLabel());
        }

        [Test]
        public void TimeOfDayLabel_Hour19_ReturnsEvening()
        {
            var ctx = new AIRequestContext { currentHour = 19 };
            Assert.AreEqual("傍晚", ctx.TimeOfDayLabel());
        }

        [Test]
        public void LastSessionDurationLabel_LessThanOneMinute_ReturnsSpecialLabel()
        {
            var ctx = new AIRequestContext { lastSessionDurationSeconds = 30f };
            Assert.AreEqual("不到一分钟", ctx.LastSessionDurationLabel());
        }

        [Test]
        public void LastSessionDurationLabel_ExactlyOneMinute_ReturnsOneMinute()
        {
            var ctx = new AIRequestContext { lastSessionDurationSeconds = 60f };
            Assert.AreEqual("1 分钟", ctx.LastSessionDurationLabel());
        }

        [Test]
        public void LastSessionDurationLabel_FortyFiveMinutes_ReturnsCorrectLabel()
        {
            var ctx = new AIRequestContext { lastSessionDurationSeconds = 2700f };
            Assert.AreEqual("45 分钟", ctx.LastSessionDurationLabel());
        }
    }
}
