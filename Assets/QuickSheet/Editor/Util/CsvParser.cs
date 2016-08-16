using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnityQuickSheet
{
    public enum TokenType
    {
        Comma,
        Quote,
        Value
    }

    public class Token
    {
        public Token(TokenType type, string value)
        {
            Value = value;
            Type = type;
        }

        public String Value { get; private set; }
        public TokenType Type { get; private set; }
    }

    public class StreamTokenizer : IEnumerable<Token>
    {
        private TextReader _reader;

        public StreamTokenizer(TextReader reader)
        {
            _reader = reader;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            String line;
            StringBuilder value = new StringBuilder();

            while ((line = _reader.ReadLine()) != null)
            {
                foreach (Char c in line)
                {
                    switch (c)
                    {
                        case '\'':
                        case '"':
                            if (value.Length > 0)
                            {
                                yield return new Token(TokenType.Value, value.ToString());
                                value.Length = 0;
                            }
                            yield return new Token(TokenType.Quote, c.ToString());
                            break;
                        case ',':
                            if (value.Length > 0)
                            {
                                yield return new Token(TokenType.Value, value.ToString());
                                value.Length = 0;
                            }
                            yield return new Token(TokenType.Comma, c.ToString());
                            break;
                        default:
                            value.Append(c);
                            break;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CsvParser : IEnumerable<String>
    {
        private StreamTokenizer _tokenizer;

        public CsvParser(Stream data)
        {
            _tokenizer = new StreamTokenizer(new StreamReader(data));
        }

        public CsvParser(String data)
        {
            _tokenizer = new StreamTokenizer(new StringReader(data));
        }

        public IEnumerator<string> GetEnumerator()
        {
            Boolean inQuote = false;
            StringBuilder result = new StringBuilder();

            foreach (Token token in _tokenizer)
            {
                switch (token.Type)
                {
                    case TokenType.Comma:
                        if (inQuote)
                        {
                            result.Append(token.Value);
                        }
                        else
                        {
                            yield return result.ToString();
                            result.Length = 0;
                        }
                        break;
                    case TokenType.Quote:
                        // Toggle quote state
                        inQuote = !inQuote;
                        break;
                    case TokenType.Value:
                        result.Append(token.Value);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown token type: " + token.Type);
                }
            }

            if (result.Length > 0)
            {
                yield return result.ToString();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}