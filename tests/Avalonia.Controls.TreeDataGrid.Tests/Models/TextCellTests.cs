using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests.Models
{
    public class TextCellTests
    {
        [AvaloniaFact(Timeout = 10000)]
        public void Value_Is_Initially_Read_From_String()
        {
            var binding = new BehaviorSubject<BindingValue<string>>("initial");
            var target = new TextCell<string>(binding, true);

            Assert.Equal("initial", target.Text);
            Assert.Equal("initial", target.Value);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Modified_Value_Is_Written_To_Binding()
        {
            var binding = new BehaviorSubject<BindingValue<string>>("initial");
            var target = new TextCell<string>(binding, false);
            var result = new List<string>();

            binding.Subscribe(x => result.Add(x.Value));
            target.Value = "new";

            Assert.Equal(new[] { "initial", "new" }, result);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Modified_Text_Is_Written_To_Binding()
        {
            var binding = new BehaviorSubject<BindingValue<string>>("initial");
            var target = new TextCell<string>(binding, false);
            var result = new List<string>();

            binding.Subscribe(x => result.Add(x.Value));
            target.Text = "new";

            Assert.Equal(new[] { "initial", "new" }, result);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Modified_Value_Is_Written_To_Binding_On_EndEdit()
        {
            var binding = new BehaviorSubject<BindingValue<string>>("initial");
            var target = new TextCell<string>(binding, false);
            var result = new List<string>();

            binding.Subscribe(x => result.Add(x.Value));

            target.BeginEdit();
            target.Text = "new";

            Assert.Equal("new", target.Text);
            Assert.Equal("new", target.Value);
            Assert.Equal(new[] { "initial"}, result);

            target.EndEdit();

            Assert.Equal("new", target.Text);
            Assert.Equal("new", target.Value);
            Assert.Equal(new[] { "initial", "new" }, result);
        }

        [AvaloniaFact(Timeout = 10000)]
        public void Modified_Value_Is_Not_Written_To_Binding_On_CancelEdit()
        {
            var binding = new BehaviorSubject<BindingValue<string>>("initial");
            var target = new TextCell<string>(binding, false);
            var result = new List<string>();

            binding.Subscribe(x => result.Add(x.Value));

            target.BeginEdit();
            target.Text = "new";

            Assert.Equal("new", target.Text);
            Assert.Equal("new", target.Value);
            Assert.Equal(new[] { "initial" }, result);

            target.CancelEdit();

            Assert.Equal("initial", target.Text);
            Assert.Equal("initial", target.Value);
            Assert.Equal(new[] { "initial" }, result);
        }
    }
}
