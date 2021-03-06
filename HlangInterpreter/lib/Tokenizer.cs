﻿using System.Collections.Generic;
using System.Globalization;
using HlangInterpreter.Errors;
using HlangInterpreter.Enums;

namespace HlangInterpreter.Lib
{
    /// <summary>
    /// Helper class to create´tokens out of a code sequence
    /// </summary>
    public class Tokenizer
    {

        public List<Token> Tokens { get; set; } = new List<Token>();
        private readonly Scanner _scanner;
        private readonly Dictionary<string, TokenType> _keywords;
        // Stack to keep track of indentation and dedentation
        private readonly Stack<int> _indentStack;
        private readonly int _tabSize = 8;
        public Tokenizer(Scanner scanner)
        {
            this._indentStack = new Stack<int>();
            // The default indentation level that will never be popped
            _indentStack.Push(0);
            this._scanner = scanner;
            // All the keywords of Hlang
            this._keywords = new Dictionary<string, TokenType>()
            {
                {"and", TokenType.AND },
                {"else", TokenType.ELSE },
                {"false", TokenType.FALSE},
                {"true", TokenType.TRUE},
                {"function", TokenType.FUNCTION },
                {"class", TokenType.CLASS},
                {"for", TokenType.FOR },
                {"each", TokenType.EACH},
                {"if", TokenType.IF },
                {"nothing", TokenType.NOTHING},
                {"while", TokenType.WHILE },
                {"print", TokenType.PRINT},
                {"return", TokenType.RETURN},
                {"equal", TokenType.EQUAL},
                {"is", TokenType.IS},
                {"not", TokenType.NOT},
                {"by", TokenType.BY },
                {"divide", TokenType.DIVIDE},
                {"multiply", TokenType.MULTIPLY},
                {"add", TokenType.ADD},
                {"subtract", TokenType.SUBTRACT},
                {"plus", TokenType.ADD},
                {"minus", TokenType.SUBTRACT},
                {"modulus", TokenType.MODULUS},
                {"greater", TokenType.GREATER},
                {"import", TokenType.IMPORT },
                {"from", TokenType.FROM },
                {"export", TokenType.EXPORT },
                {"less", TokenType.LESS},
                {"than", TokenType.THAN },
                {"then", TokenType.THEN },
                {"this", TokenType.THIS },
                {"extends", TokenType.EXTENDS },
                {"define", TokenType.DEFINE },
                {"parent", TokenType.PARENT },
                {"static", TokenType.STATIC },
                {"private", TokenType.PRIVATE },
                {"to", TokenType.TO },
                {"in", TokenType.IN },
                {"break", TokenType.BREAK },
                {"lambda", TokenType.LAMBDA },
                {"increment", TokenType.INCREMENT },
                {"incremented", TokenType.INCREMENT},
                {"decrement", TokenType.DECREMENT },
                {"decremented", TokenType.DECREMENT },
                {"or", TokenType.OR },
                {"type", TokenType.TYPE },
                {"complement", TokenType.COMPLEMENT },
                {"of", TokenType.OF }
            };

        }
        /// <summary>
        /// Get the tokens out of the code sequence
        /// </summary>
        /// <returns>A list of tokens</returns>
        public List<Token> GetTokens()
        {
            while (!_scanner.IsEof())
            {
                _scanner.Start = _scanner.Position;
                ReadNextToken();
            }
            Tokens.Add(new Token(TokenType.EOF, "", null, _scanner.Line));
            return Tokens;
        }
        /// <summary>
        /// Reads the next token in the sequence
        /// </summary>
        private void ReadNextToken()
        {
            if (_scanner.IsAbol)
            {
                GetIndentLevel();
                return;
            }

            // Get next token out of current character
            char c = _scanner.MoveToNextChar();
            switch (c)
            {
                case '\t': AddToken(TokenType.INDENT); break;
                case ' ': break;
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '/': SkipComment(); break;
                case '[': AddToken(TokenType.LEFT_BRACKET); break;
                case ']': AddToken(TokenType.RIGHT_BRACKET); break;
                case ',': AddToken(TokenType.COMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '\r': break;
                case '\n':
                    _scanner.IsAbol = true;
                    _scanner.Line++;
                    break;
                case '"': ReadString(); break;
                case '_':
                default:
                    if (IsDigit(c))
                    {
                        ReadNumber();
                    } else if (IsAlphabet(c))
                    {
                       ReadWholeWord();
                    }
                    else
                    {
                        throw new SyntaxError(_scanner.Line, "Can't process character");
                    }
                    break;
            }
        }
        /// <summary>
        /// Get's the indentation level of the current line
        /// </summary>
        private void GetIndentLevel()
        {
            int indent = 0;
            // Each time there is a space or a tab, we indent the column
            for (; ; )
            {
                char character = _scanner.PeekCurrentChar();
                if (character == ' ')
                {
                    indent++;
                    _scanner.MoveToNextChar();
                }
                else if (character == '\t')
                {
                    indent = (indent / _tabSize + 1) * _tabSize;
                    _scanner.MoveToNextChar();
                }
                else
                {
                    break;
                }
            }
            
            if (indent > 0 &&_scanner.Line == 0)
            {
                throw new SyntaxError(_scanner.Line, "Unexpected start of program indent");
            }

            // If the indentation is bigger than top of stack
            // Push indentation an create indent token
            // Otherwise create dedent tokens until the top is equal to indentation
            if (indent > _indentStack.Peek())
            {
                _indentStack.Push(indent);
                AddToken(TokenType.INDENT);
                
            }
            else
            {
                while (indent < _indentStack.Peek())
                {
                    _indentStack.Pop();
                    AddToken(TokenType.DEDENT);
                }

                if (indent != _indentStack.Peek())
                {
                    throw new SyntaxError(_scanner.Line, "Unexpected indent");
                }
            }
            _scanner.IsAbol = false;
        }

        /// <summary>
        /// Skip both  //  and /* */ comments
        /// </summary>
        private void SkipComment()
        {
            if (_scanner.Match('/'))
            {
                while (_scanner.PeekCurrentChar() != '\n' && !_scanner.IsEof()) _scanner.MoveToNextChar();
            }
            else if (_scanner.Match('*'))
            {
                while (true)
                {
                    if (_scanner.Match('*') && _scanner.Match('/'))
                    {
                        break;
                    }
                    _scanner.MoveToNextChar();
                }
            }
        }

        /// <summary>
        /// Reads the whole number in the sequence of characters and adds the number token
        /// </summary>
        private void ReadNumber()
        {
            // While the character is a digit, move forward
            while (IsDigit(_scanner.PeekCurrentChar())) _scanner.MoveToNextChar();

            // If we hit a period then continue
            if (_scanner.PeekCurrentChar() == '.' && IsDigit(_scanner.PeekNextChar()))
            {
                _scanner.MoveToNextChar();
                while (IsDigit(_scanner.PeekCurrentChar())) _scanner.MoveToNextChar();
            }

            // parse the literal and add to a number token
            var value = _scanner.GetStartToCurrent();

            AddToken(TokenType.NUMBER, double.Parse(value, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Reads a string literal in the sequence of characters and adds the string token
        /// </summary>
        private void ReadString()
        {
            // Move forward while we are not at the end of the string or end of file/sequence
            while(_scanner.PeekCurrentChar() != '"' && !_scanner.IsEof())
            {
                if (_scanner.PeekCurrentChar() == '\n') _scanner.Line++;
                _scanner.MoveToNextChar();
            }

            if (_scanner.IsEof())
            {
                throw new SyntaxError(_scanner.Line, "Unterminated string");
            }

            _scanner.MoveToNextChar();
            // Extract the string without the quotation marks
            string str = _scanner.CodeStr.Substring(_scanner.Start + 1, (_scanner.Position - _scanner.Start) - 2);
            AddToken(TokenType.STRING, str);
            
        }
        /// <summary>
        /// Reads a whole word (not string literal) in the sequence and adds the proper token
        /// </summary>
        private void ReadWholeWord()
        {
            // According to C#'s Roselyn repository using a switch block like this is much more performance efficent
            bool keepRunning = true;
            while (keepRunning)
            {
                switch (_scanner.PeekCurrentChar())
                {
                    case '\0':
                    case ' ':
                    case '\r':
                    case '\n':
                    case '\t':
                    case '!':
                    case '%':
                    case '(':
                    case ')':
                    case '*':
                    case '+':
                    case ',':
                    case '-':
                    case '.':
                    case '/':
                    case ':':
                    case ';':
                    case '<':
                    case '=':
                    case '>':
                    case '?':
                    case '[':
                    case ']':
                    case '^':
                    case '{':
                    case '|':
                    case '}':
                    case '~':
                    case '"':
                    case '\'':
                        keepRunning = false;
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    case '_':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                        _scanner.MoveToNextChar();
                        break;
                    default:
                        break;
                }
            }
           
            //while ((char.IsLetter(_scanner.PeekCurrentChar()) || char.IsDigit(_scanner.PeekCurrentChar())) && !_scanner.IsEof()) _scanner.MoveToNextChar();
            //while (!char.(_scanner.PeekCurrentChar()) && !_scanner.IsEof()) _scanner.MoveToNextChar();
            var value = _scanner.GetStartToCurrent();
            // If the value does not exist keywords, it must be an identifer
            if (!_keywords.TryGetValue(value, out TokenType type)) type = TokenType.IDENTIFER;
            AddToken(type);
        }
        /// <summary>
        /// Check if a character is a digit
        /// </summary>
        /// <param name="ch">Character to check</param>
        private bool IsDigit(char ch)
        {
            return char.IsDigit(ch);
        }

        /// <summary>
        /// Check if a character is an alphabet
        /// </summary>
        /// <param name="c">Character to check</param>
        private bool IsAlphabet(char c)
        {
            return char.IsLetter(c);
        }

        /// <summary>
        /// Add token to the token list
        /// </summary>
        /// <param name="type">Type of token to add</param>
        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        /// <summary>
        /// Add token to the token list
        /// </summary>
        /// <param name="type">Type of token to add</param>
        /// <param name="literal">Literal of the token</param>
        private void AddToken(TokenType  type, object literal)
        {
            string text = _scanner.GetStartToCurrent();
            Tokens.Add(new Token(type, text, literal, _scanner.Line));
        }
    }
}
