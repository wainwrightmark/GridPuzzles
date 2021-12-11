using System;
using System.Threading.Tasks;
using SVGElements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SVGHelper;

public interface IWebSVGEventHandler : ISVGEventHandler
{
    void AddEventCallback(RenderTreeBuilder renderTreeBuilder, int i, object receiver);
}

/// <summary>
/// Handles events such as onClick and onKeyPress
/// </summary>
public abstract class SVGEventHandler : ISVGEventHandler
{
    public static ISVGEventHandler OnKeyPressAsync(Func<KeyboardEventArgs, Task> func) => new SVGEventHandler1<KeyboardEventArgs>("onkeypress", func);
    public static ISVGEventHandler OnKeyPress(Action<KeyboardEventArgs> func) => new SVGEventHandler1<KeyboardEventArgs>("onkeypress", func);

    public static ISVGEventHandler OnClickAsync(Func<MouseEventArgs, Task> func) => new SVGEventHandler1<MouseEventArgs>("onclick", func);
    public static ISVGEventHandler OnClick(Action<MouseEventArgs> func) => new SVGEventHandler1<MouseEventArgs>("onclick", func);


    private class SVGEventHandler1<T> : IWebSVGEventHandler
    {
        public SVGEventHandler1(string name, Func<T, Task> asyncFunc)
        {
            Name = name;
            _asyncFunc = asyncFunc;
        }

        public SVGEventHandler1(string name, Action<T> action)
        {
            Name = name;
            _action = action;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public void AddEventCallback(RenderTreeBuilder renderTreeBuilder, int i, object receiver)
        {
            renderTreeBuilder.AddAttribute(
                i,
                Name,
                CreateEventCallback(receiver)
            );
        }

        private readonly Func<T, Task>? _asyncFunc;
        private readonly Action<T>? _action;

        private EventCallback<T> CreateEventCallback(object receiver)
        {
            if (_asyncFunc is not null)
            {
                return EventCallback.Factory.Create<T>(receiver, e => _asyncFunc(e));
            }

            if (_action is not null)
            {
                return EventCallback.Factory.Create<T>(receiver, e => _action(e));
            }

            throw new Exception("Both ASyncFunc and Action were null");
        }
    }

    /// <inheritdoc />
    public abstract string Name { get; }
}