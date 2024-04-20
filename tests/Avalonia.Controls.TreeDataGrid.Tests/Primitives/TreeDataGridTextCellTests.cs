using System.Reactive.Subjects;

using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless.XUnit;

using Xunit;

namespace Avalonia.Controls.TreeDataGridTests.Primitives
{
    public class TreeDataGridTextCellTests
    {
        [AvaloniaTheory(Timeout = 10000)]
        [InlineData(10.1, "{0:n3}", "10.100")]
        [InlineData(20.12345, "{0:n2}", "20.12")]
        [InlineData(5050.50, "{0:n2}", "5,050.50")]
        public void Formats_Double_Properly(double input, string formatString, string expected)
        {
            var cell = SetupCellForType(input, formatString);
            Assert.Equal(expected, cell.Value);
        }

        [AvaloniaTheory(Timeout = 10000)]
        [InlineData(1_000_000, "{0:n0}", "1,000,000")]
        [InlineData(2032, "{0:n2}", "2,032.00")]
        [InlineData(5050, "{0}", "5050")]
        [InlineData(5050, "{0:c}", "5050.00")]
        public void Formats_Int_Properly(int input, string formatString, string expected)
        {
            var cell = SetupCellForType(input, formatString);
            Assert.Equal(expected, cell.Value);
        }

        private static TreeDataGridTextCell SetupCellForType<T>(T input, string? formatString)
        {
            var subject = new Subject<BindingValue<T>>();
            var cell = new TreeDataGridTextCell();
            var options = new TextColumnOptions<T>();
            if (formatString is not null)
            {
                options.StringFormat = formatString;
            }
            var model = new TextCell<T>(subject, true, options);
            subject.OnNext(new BindingValue<T>(input));
            cell.Realize(new TestElementFactory(), null, model, 0, 0);
            return cell;
        }
    }
}
