using System;
using System.Collections.Generic;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters
{
    /// <summary>
    /// Manager that holds all of the parameter data
    /// </summary>
    public interface IParameterManager
    {
        /// <summary>
        /// Get a particular parameter object by identifier.
        /// </summary>
        /// <param name="identifier">identifier to query</param>
        /// <typeparam name="T">type to query</typeparam>
        /// <returns>the parameter if it exists</returns>
        T Get<T>(string identifier) where T : class, IBaseInfo;

        /// <summary>
        /// Get a particular parameter object by identifier.
        /// </summary>
        /// <param name="type">type of object to return. This is expected to be a subinterface of IBaseInfo</param>
        /// <param name="identifier">identifier to query</param>
        /// <returns>object of type IBaseInfo, and implicitly of type Type</returns>
        IBaseInfo Get(string identifier, Type type);

        /// <summary>
        /// Get a particular parameter object by guid.
        /// </summary>
        /// <param name="guid">asset guid to query</param>
        /// <typeparam name="T">type to query</typeparam>
        /// <returns>the parameter if it exists</returns>
        T GetWithGUID<T>(string guid) where T : class, IBaseInfo;

        /// <summary>
        /// Get a struct parameter object by identifier
        /// </summary>
        /// <param name="identifier">internal identifier of the struct</param>
        /// <typeparam name="T">struct type</typeparam>
        /// <returns>the struct if it exists</returns>
        T GetStructWithGuid<T>(string guid) where T : class, IBaseStruct;

        /// <summary>
        /// Get a list of parameter objects of the same type.
        ///
        /// There is no guaranteed order.
        /// </summary>
        /// <typeparam name="T">type to query</typeparam>
        /// <returns>enumerator for all parameters the type</returns>
        IEnumerable<T> Get<T>() where T : class, IBaseInfo;

        /// <summary>
        /// Get a list of parameter objects of the same type in
        /// identifier ascending order.
        ///
        /// This call is more expensive than Get() due to extra sorting overhead.
        /// </summary>
        /// <typeparam name="T">type to query</typeparam>
        /// <returns>enumerator for all parameters the type</returns>
        IEnumerable<T> GetSorted<T>() where T : class, IBaseInfo;

        /// <summary>
        /// Gets or sets whether any ParameterManager's Get methods are safe to be called.  Get can be unsafe
        /// if they're currently waiting to be modified via AB testing.
        /// </summary>
        bool IsGettingSafe { get; set; }

        /// <summary>
        /// Gets whether or not any parameter has ever been retrieved from this ParameterManager. The purpose of this
        /// property is to know if it is safe to apply a parameter override (like from an AB Test). It would not be
        /// safe to apply a parameter override if this property is 'true', because we can't know if that parameter's
        /// value has been copied somewhere and so would be out-of-date if we applied an override.
        /// </summary>
        bool HasGetBeenCalled { get; }
    }
}
