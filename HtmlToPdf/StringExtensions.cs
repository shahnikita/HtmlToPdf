using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlToPdf
{
    public static class StringExtensions
    {
        public static string CloseTags(this string html)
        {
            if (String.IsNullOrWhiteSpace(html))
            {
                return html;
            }

            return new TagCloser().CloseTags(html);
        }

        class TagCloser
        {
            private Stack<string> _openTags = new Stack<string>();
            private string _html;
            private int _currentCharIndex;
            private bool Eof
            {
                get { return _currentCharIndex == _html.Length - 1; }
            }

            private int _pendingTagNameStartIndex;
            private string _pendingTagName;
            private int _currentEndTagStartIndex;

            private State _state;

            public string CloseTags(string html)
            {
                _html = html;
                _currentCharIndex = -1;

                while (!Eof)
                {
                    var ch = ReadNext();

                    if (_state == State.InTagName)
                    {
                        // Self closing
                        if (ch == '/' && PeekNext() == '>')
                        {
                            ReadNext();
                            _state = _openTags.Count > 0 ? State.InsideTag : State.None;
                            continue;
                        }

                        if (ch == ' ' || ch == '>')
                        {
                            _pendingTagName = _html.Substring(_pendingTagNameStartIndex, _currentCharIndex - _pendingTagNameStartIndex);
                        }

                        if (ch == ' ')
                        {
                            _state = State.InAttributes;
                        }
                        else if (ch == '>')
                        {
                            _openTags.Push(_pendingTagName);
                            _state = State.InsideTag;
                        }
                    }
                    else if (_state == State.InsideTag)
                    {
                        if (ch == '<' && PeekNext() == '/')
                        {
                            _currentEndTagStartIndex = _currentCharIndex;
                            ReadNext();
                            _state = State.InEndTag;
                            continue;
                        }
                        if (ch == '<')
                        {
                            _state = State.InTagName;
                            _pendingTagNameStartIndex = _currentCharIndex + 1;
                            continue;
                        }
                    }
                    else if (_state == State.InEndTag)
                    {
                        if (ch == '>')
                        {
                            _openTags.Pop();
                            _state = _openTags.Count > 0 ? State.InsideTag : State.None;
                        }
                    }
                    else
                    {
                        if (ch == '<')
                        {
                            _state = State.InTagName;
                            _pendingTagNameStartIndex = _currentCharIndex + 1;
                            continue;
                        }
                    }
                }

                // Broken start tag
                if (_state == State.InTagName || _state == State.InAttributes)
                {
                    _html = _html.Substring(0, _pendingTagNameStartIndex - 1);
                }
                // Broken end tag
                else if (_state == State.InEndTag)
                {
                    _html = _html.Substring(0, _currentEndTagStartIndex);
                }

                var sb = new StringBuilder();
                sb.Append(_html);

                while (_openTags.Count > 0)
                {
                    var tag = _openTags.Pop();
                    sb.Append("</" + tag + ">");
                }

                return sb.ToString();
            }

            private char ReadNext()
            {
                return Eof ? '\0' : _html[++_currentCharIndex];
            }

            private char PeekNext()
            {
                return Eof ? '\0' : _html[_currentCharIndex + 1];
            }

            enum State
            {
                None, InTagName, InAttributes, InsideTag, InEndTag
            }
        }
    }
}
