using System;

namespace lingvo.core
{
    /// <summary>
    /// 
    /// </summary>
    public static class StringsHelper
    {
        /// <summary>
        /// 
        /// </summary>
        unsafe public static string ToUpperInvariant( string value )
        {
            var len = value.Length;
            if ( 0 < len )
            {
                var valueUpper = string.Copy( value );
                fixed ( char* valueUpper_ptr = valueUpper )
                {
                    for ( int i = 0; i < len; i++ )
                    {
                        var ptr = valueUpper_ptr + i;
                        *ptr = *(xlat_Unsafe.Inst._UPPER_INVARIANT_MAP + *ptr);
                    }
                }
                return (valueUpper);
            }
            return (string.Empty);
        }
        unsafe public static void   ToUpperInvariant( char* wordFrom, char* bufferTo )
        {
            for ( ; ; wordFrom++, bufferTo++ )
            {
                var ch = *wordFrom;
                *bufferTo = *(xlat_Unsafe.Inst._UPPER_INVARIANT_MAP + ch);
                if ( ch == '\0' )
                    return;
            }            
        }
        unsafe public static void   ToUpperInvariantInPlace( string value )
        {
            fixed ( char* value_ptr = value )
            {
                ToUpperInvariantInPlace( value_ptr );
            }
        }
        unsafe public static void   ToUpperInvariantInPlace( char* word )
        {
            for ( ; ; word++ )
            {
                var ch = *word;
                if ( ch == '\0' )
                    return;
                *word = *(xlat_Unsafe.Inst._UPPER_INVARIANT_MAP + ch);
            }
        }

        public static string ToLowerInvariant( string value )
        {
            return (value.ToLowerInvariant());
        }

        /// проверка эквивалентности строк
        unsafe public static bool IsEqual( string first, string second )
        {
            int length = first.Length;
            if ( length != second.Length )
            {
                return (false);
            }
            if ( length == 0 )
            {
                return (true);
            }

            fixed ( char* first_ptr  = first )
            fixed ( char* second_ptr = second )
            {
                for ( int i = 0; i < length; i++ )
                {
                    if ( *(first_ptr + i) != *(second_ptr + i) ) //if ( GetLetter( first, i ) != GetLetter( second, i ) )
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }
        unsafe public static bool IsEqual( string first, char* second_ptr, int secondLength )
        {
            /*
            if ( first.Length != secondLength )
            {
                return (false);
            }
            if ( secondLength == 0 )
            {
                return (true);
            }
            */

            fixed ( char* first_ptr  = first )
            {
                for ( int i = 0; i < secondLength; i++ )
                {
                    if ( *(first_ptr + i) != *(second_ptr + i) )
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }

        unsafe public static bool IsEqual( string first, int firstIndex, string second )
        {
            int length = first.Length - firstIndex;
            if ( length != second.Length )
            {
                return (false);
            }
            if ( length == 0 )
            {
                return (true);
            }

            fixed ( char* first_base = first  )
            fixed ( char* second_ptr = second )
            {
                char* first_ptr = first_base + firstIndex;
                for ( int i = 0; i < length; i++ )
                {
                    if ( *(first_ptr + i) != *(second_ptr + i) ) //if ( GetLetter( first, i ) != GetLetter( second, i ) )
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }
        unsafe public static bool IsEqual( string first, int firstIndex, char* second_ptr, int secondLength )
        {
            int length = first.Length - firstIndex;
            if ( length != secondLength )
            {
                return (false);
            }
            if ( secondLength == 0 )
            {
                return (true);
            }

            fixed ( char* first_base = first  )
            {
                char* first_ptr = first_base + firstIndex;
                for ( int i = 0; i < secondLength; i++ )
                {
                    if ( *(first_ptr + i) != *(second_ptr + i) ) //if ( GetLetter( first, i ) != GetLetter( second, i ) )
                    {
                        return (false);
                    }
                }
            }
            return (true);
        }

        unsafe public static int GetLength( char* _base )
        {
            for ( var ptr = _base; ; ptr++ )
            {
                if ( *ptr == '\0' )
                {
                    return ((int)(ptr - _base));
                }
            }
        }
        unsafe public static int GetLength( IntPtr _base )
        {
            return (GetLength( (char*) _base ));
        }

        unsafe public static string CreateWordForm( string _base, char* morphoFormEnding )
        {
            var endingLength = GetLength( morphoFormEnding );
            if ( endingLength == 0 )
                return (_base);

            var wordForm = new string( '\0', _base.Length + endingLength );
            fixed ( char* wordForm_ptr = wordForm )
            fixed ( char* _base_ptr    = _base )
            {
                var wf_ptr = wordForm_ptr;
                var ptr    = _base_ptr;
                for ( ; ; )
                {
                    var ch = *(ptr++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }

                for ( ; ; )
                {
                    var ch = *(morphoFormEnding++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }
            }
            return (wordForm);
        }
        unsafe public static string CreateWordForm( char*  _base, char* morphoFormEnding )
        {            
            var endingLength = GetLength( morphoFormEnding );
            if ( endingLength == 0 )
            {
                return (new string( _base ));
            }

            var baseLength = GetLength( _base );

            var wordForm = new string( '\0', baseLength + endingLength );
            fixed ( char* wordForm_ptr = wordForm )
            {
                var wf_ptr = wordForm_ptr;
                for ( ; ; )
                {
                    var ch = *(_base++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }

                for ( ; ; )
                {
                    var ch = *(morphoFormEnding++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }
            }
            return (wordForm);
        }

        unsafe public static string ToString( char* value )
        {
            if ( value == null )
            {
                return (null);
            }

            var length = GetLength( value );
            if ( length == 0 )
            {
                return (string.Empty);
            }

            var str = new string( '\0', length );
            fixed ( char* str_ptr = str )
            {                
                for ( var wf_ptr = str_ptr; ; )
                {
                    var ch = *(value++);
                    if ( ch == '\0' )
                        break;
                    *(wf_ptr++) = ch;
                }
            }
            return (str);
        }
        unsafe public static string ToString( IntPtr value )
        {
            return (ToString( (char*) value ));
        }
    }
}
