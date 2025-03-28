using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Data;
using Avalonia.Experimental.Data;
using Xunit;

namespace Avalonia.Controls.TreeDataGridTests.Bindings;

public class TypedBindingTests
{
    [Fact]
    public void OneWay_Binding_Single_Link_Should_Get_Value()
    {
        // Arrange
        var source = new TestViewModel { Name = "Test" };
        var target = new TestTarget();
        
        // Create a binding with a single link (source.Name)
        var binding = TypedBinding<TestViewModel>.OneWay(vm => vm.Name);
        var expression = binding.Instance(source);
        
        // Act
        target.Bind(TestTarget.TextProperty, expression);
        
        // Assert
        Assert.Equal("Test", target.Text);
    }
    
    [Fact]
    public void OneWay_Binding_Single_Link_Should_Listen_To_Changes()
    {
        // Arrange
        var source = new TestViewModel { Name = "Test" };
        var target = new TestTarget();
        
        // Create a binding with a single link (source.Name)
        var binding = TypedBinding<TestViewModel>.OneWay(vm => vm.Name);
        var expression = binding.Instance(source);
        
        // Act
        target.Bind(TestTarget.TextProperty, expression);
        source.Name = "Updated";
        
        // Assert
        Assert.Equal("Updated", target.Text);
    }
    
    [Fact]
    public void TwoWay_Binding_Single_Link_Should_Listen_And_Write()
    {
        // Arrange
        var source = new TestViewModel { Name = "Test" };
        var target = new TestTarget();
        
        // Create a binding with a single link (source.Name)
        var binding = TypedBinding<TestViewModel>.TwoWay(vm => vm.Name);
        var expression = binding.Instance(source, BindingMode.TwoWay);
        
        // Act - set up binding
        target.Bind(TestTarget.TextProperty, expression);
        
        // Act - change source
        source.Name = "Updated";
        
        // Assert
        Assert.Equal("Updated", target.Text);
        
        // Act - change target
        target.Text = "UpdatedFromTarget";
        binding.Write!.Invoke(source, target.Text);
        
        // Assert
        Assert.Equal("UpdatedFromTarget", source.Name);
    }
    
    [Fact]
    public void OneWay_Binding_Two_Links_Should_Get_Value()
    {
        // Arrange
        var child = new TestViewModel { Name = "Child" };
        var source = new TestViewModel { Name = "Parent", Child = child };
        var target = new TestTarget();
        
        // Create a binding with two links (source.Child.Name)
        var binding = TypedBinding<TestViewModel>.OneWay(vm => vm.Child!.Name);
        var expression = binding.Instance(source);
        
        // Act
        target.Bind(TestTarget.TextProperty, expression);
        
        // Assert
        Assert.Equal("Child", target.Text);
    }
    
    [Fact]
    public void OneWay_Binding_Two_Links_Should_Listen_To_Changes()
    {
        // Arrange
        var child = new TestViewModel { Name = "Child" };
        var source = new TestViewModel { Name = "Parent", Child = child };
        var target = new TestTarget();
        
        // Create a binding with two links (source.Child.Name)
        var binding = TypedBinding<TestViewModel>.OneWay(vm => vm.Child!.Name);
        var expression = binding.Instance(source);
        
        // Act - initialize binding
        target.Bind(TestTarget.TextProperty, expression);
        
        // Act - update leaf property
        child.Name = "UpdatedChild";
        
        // Assert
        Assert.Equal("UpdatedChild", target.Text);
        
        // Act - update intermediate property
        var newChild = new TestViewModel { Name = "NewChild" };
        source.Child = newChild;
        
        // Assert
        Assert.Equal("NewChild", target.Text);
    }
    
    [Fact]
    public void TwoWay_Binding_Two_Links_Should_Listen_And_Write()
    {
        // Arrange
        var child = new TestViewModel { Name = "Child" };
        var source = new TestViewModel { Name = "Parent", Child = child };
        var target = new TestTarget();
        
        // Create a binding with two links (source.Child.Name)
        var binding = TypedBinding<TestViewModel>.TwoWay(vm => vm.Child!.Name);
        var expression = binding.Instance(source, BindingMode.TwoWay);
        
        // Act - initialize binding
        target.Bind(TestTarget.TextProperty, expression);
        
        // Act - update from source
        child.Name = "UpdatedChild";
        
        // Assert
        Assert.Equal("UpdatedChild", target.Text);
        
        // Act - update from target
        target.Text = "UpdatedFromTarget";
        binding.Write!.Invoke(source, target.Text);
        
        // Assert
        Assert.Equal("UpdatedFromTarget", child.Name);
        
        // Act - change intermediate link
        var newChild = new TestViewModel { Name = "NewChild" };
        source.Child = newChild;
        
        // Assert
        Assert.Equal("NewChild", target.Text);
        
        // Act - update from target after changing intermediate
        target.Text = "FinalUpdate";
        binding.Write!.Invoke(source, target.Text);
        
        // Assert
        Assert.Equal("FinalUpdate", newChild.Name);
    }
    
    [Fact]
    public void OneWay_Binding_Three_Links_Should_Get_Value()
    {
        // Arrange
        var grandChild = new TestViewModel { Name = "GrandChild" };
        var child = new TestViewModel { Name = "Child", Child = grandChild };
        var source = new TestViewModel { Name = "Parent", Child = child };
        var target = new TestTarget();
        
        // Create a binding with three links (source.Child.Child.Name)
        var binding = TypedBinding<TestViewModel>.OneWay(vm => vm.Child!.Child!.Name);
        var expression = binding.Instance(source);
        
        // Act
        target.Bind(TestTarget.TextProperty, expression);
        
        // Assert
        Assert.Equal("GrandChild", target.Text);
    }
    
    [Fact]
    public void OneWay_Binding_Three_Links_Should_Listen_To_Changes()
    {
        // Arrange
        var grandChild = new TestViewModel { Name = "GrandChild" };
        var child = new TestViewModel { Name = "Child", Child = grandChild };
        var source = new TestViewModel { Name = "Parent", Child = child };
        var target = new TestTarget();
        
        // Create a binding with three links (source.Child.Child.Name)
        var binding = TypedBinding<TestViewModel>.OneWay(vm => vm.Child!.Child!.Name);
        var expression = binding.Instance(source);
        
        // Act - initialize binding
        target.Bind(TestTarget.TextProperty, expression);
        
        // Act - update leaf property
        grandChild.Name = "UpdatedGrandChild";
        
        // Assert
        Assert.Equal("UpdatedGrandChild", target.Text);
        
        // Act - update middle property
        var newGrandChild = new TestViewModel { Name = "NewGrandChild" };
        child.Child = newGrandChild;
        
        // Assert
        Assert.Equal("NewGrandChild", target.Text);
        
        // Act - update root property
        var newChildWithGrandChild = new TestViewModel { 
            Name = "NewChild", 
            Child = new TestViewModel { Name = "NewestGrandChild" } 
        };
        source.Child = newChildWithGrandChild;
        
        // Assert
        Assert.Equal("NewestGrandChild", target.Text);
    }
    
    [Fact]
    public void TwoWay_Binding_Three_Links_Should_Listen_And_Write()
    {
        // Arrange
        var grandChild = new TestViewModel { Name = "GrandChild" };
        var child = new TestViewModel { Name = "Child", Child = grandChild };
        var source = new TestViewModel { Name = "Parent", Child = child };
        var target = new TestTarget();
        
        // Create a binding with three links (source.Child.Child.Name)
        var binding = TypedBinding<TestViewModel>.TwoWay(vm => vm.Child!.Child!.Name);
        var expression = binding.Instance(source, BindingMode.TwoWay);
        
        // Act - initialize binding
        target.Bind(TestTarget.TextProperty, expression);
        
        // Act - set from target
        target.Text = "UpdatedFromTarget";
        binding.Write!.Invoke(source, target.Text);
        
        // Assert
        Assert.Equal("UpdatedFromTarget", grandChild.Name);
        
        // Act - change intermediate node and update from target
        var newGrandChild = new TestViewModel { Name = "NewGrandChild" };
        child.Child = newGrandChild;
        
        // Assert initial update
        Assert.Equal("NewGrandChild", target.Text);
        
        // Act - update again from target
        target.Text = "AfterIntermediateChange";
        binding.Write!.Invoke(source, target.Text);
        
        // Assert
        Assert.Equal("AfterIntermediateChange", newGrandChild.Name);
    }
    
    [Fact]
    public void OneWay_Binding_Four_Links_Should_Get_Value()
    {
        // Arrange
        var greatGrandChild = new TestViewModel { Name = "GreatGrandChild" };
        var grandChild = new TestViewModel { Name = "GrandChild", Child = greatGrandChild };
        var child = new TestViewModel { Name = "Child", Child = grandChild };
        var source = new TestViewModel { Name = "Parent", Child = child };
        var target = new TestTarget();
        
        // Create a binding with four links (source.Child.Child.Child.Name)
        var binding = TypedBinding<TestViewModel>.OneWay(vm => vm.Child!.Child!.Child!.Name);
        var expression = binding.Instance(source);
        
        // Act
        target.Bind(TestTarget.TextProperty, expression);
        
        // Assert
        Assert.Equal("GreatGrandChild", target.Text);
    }
    
    [Fact]
    public void OneWay_Binding_Four_Links_Should_Listen_To_Changes()
    {
        // Arrange
        var greatGrandChild = new TestViewModel { Name = "GreatGrandChild" };
        var grandChild = new TestViewModel { Name = "GrandChild", Child = greatGrandChild };
        var child = new TestViewModel { Name = "Child", Child = grandChild };
        var source = new TestViewModel { Name = "Parent", Child = child };
        var target = new TestTarget();
        
        // Create a binding with four links (source.Child.Child.Child.Name)
        var binding = TypedBinding<TestViewModel>.OneWay(vm => vm.Child!.Child!.Child!.Name);
        var expression = binding.Instance(source);
        
        // Act - initialize binding
        target.Bind(TestTarget.TextProperty, expression);
        
        // Act - update leaf node
        greatGrandChild.Name = "UpdatedGreatGrandChild";
        
        // Assert
        Assert.Equal("UpdatedGreatGrandChild", target.Text);
        
        // Act - update middle node
        var newGreatGrandChild = new TestViewModel { Name = "NewGreatGrandChild" };
        grandChild.Child = newGreatGrandChild;
        
        // Assert
        Assert.Equal("NewGreatGrandChild", target.Text);
        
        // Act - update higher node
        var newGrandChildWithChild = new TestViewModel
        {
            Name = "NewGrandChild",
            Child = new TestViewModel { Name = "BrandNewGreatGrandChild" }
        };
        child.Child = newGrandChildWithChild;
        
        // Assert
        Assert.Equal("BrandNewGreatGrandChild", target.Text);
    }
    
    [Fact]
    public void TwoWay_Binding_Four_Links_Should_Listen_And_Write()
    {
        // Arrange
        var greatGrandChild = new TestViewModel { Name = "GreatGrandChild" };
        var grandChild = new TestViewModel { Name = "GrandChild", Child = greatGrandChild };
        var child = new TestViewModel { Name = "Child", Child = grandChild };
        var source = new TestViewModel { Name = "Parent", Child = child };
        var target = new TestTarget();
        
        // Create a binding with four links (source.Child.Child.Child.Name)
        var binding = TypedBinding<TestViewModel>.TwoWay(vm => vm.Child!.Child!.Child!.Name);
        var expression = binding.Instance(source, BindingMode.TwoWay);
        
        // Act - initialize binding
        target.Bind(TestTarget.TextProperty, expression);
        
        // Act - set from target
        target.Text = "UpdatedFromTarget";
        binding.Write!.Invoke(source, target.Text);
        
        // Assert
        Assert.Equal("UpdatedFromTarget", greatGrandChild.Name);
        
        // Act - change leaf node
        var newGreatGrandChild = new TestViewModel { Name = "NewGreatGrandChild" };
        grandChild.Child = newGreatGrandChild;
        
        // Assert
        Assert.Equal("NewGreatGrandChild", target.Text);
        
        // Act - update from target
        target.Text = "AfterLeafChange";
        binding.Write!.Invoke(source, target.Text);
        
        // Assert
        Assert.Equal("AfterLeafChange", newGreatGrandChild.Name);
        
        // Act - update middle node
        var newGrandChildWithChild = new TestViewModel
        {
            Name = "NewGrandChild",
            Child = new TestViewModel { Name = "BrandNewGreatGrandChild" }
        };
        child.Child = newGrandChildWithChild;
        
        // Assert
        Assert.Equal("BrandNewGreatGrandChild", target.Text);
        
        // Act - update from target
        target.Text = "AfterMiddleChange";
        binding.Write!.Invoke(source, target.Text);
        
        // Assert
        Assert.Equal("AfterMiddleChange", newGrandChildWithChild.Child.Name);
    }
    
    [Fact] 
    public void Binding_Should_Handle_Null_Intermediate_Values()
    {
        // Arrange
        var source = new TestViewModel { Name = "Parent", Child = null };
        var target = new TestTarget();
        
        // Create a binding that will encounter a null (source.Child.Name)
        var binding = TypedBinding<TestViewModel>.OneWay(vm => vm.Child!.Name);
        var expression = binding.Instance(source);
        
        // Act - initialize binding
        target.Bind(TestTarget.TextProperty, expression);
        
        // Assert - binding should have error state due to null reference
        Assert.Null(target.Text); // Default value since binding path fails
        
        // Act - fix the null reference
        var newChild = new TestViewModel { Name = "NewChild" };
        source.Child = newChild;
        
        // Assert - binding should work now
        Assert.Equal("NewChild", target.Text);
    }
    
    // Test view model implementing property change notifications
    private class TestViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private TestViewModel ?_child = null;
        
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        
        public TestViewModel ?Child
        {
            get => _child;
            set => SetProperty(ref _child, value);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    // Simple target class with an AvaloniaProperty
    private class TestTarget : AvaloniaObject
    {
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<TestTarget, string>(nameof(Text));
            
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}