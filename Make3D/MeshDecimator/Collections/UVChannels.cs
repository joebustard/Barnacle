﻿namespace MeshDecimator.Collections
{
    /// <summary>
    /// A collection of UV channels.
    /// </summary>
    /// <typeparam name="TVec">The UV vector type.</typeparam>
    internal sealed class UVChannels<TVec>
    {
        #region Fields

        private ResizableArray<TVec>[] channels = null;
        private TVec[][] channelsData = null;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the channel collection data.
        /// </summary>
        public TVec[][] Data
        {
            get
            {
                for (int i = 0; i < Mesh.UVChannelCount; i++)
                {
                    if (channels[i] != null)
                    {
                        channelsData[i] = channels[i].Data;
                    }
                    else
                    {
                        channelsData[i] = null;
                    }
                }
                return channelsData;
            }
        }

        /// <summary>
        /// Gets or sets a specific channel by index.
        /// </summary>
        /// <param name="index">The channel index.</param>
        public ResizableArray<TVec> this[int index]
        {
            get { return channels[index]; }
            set { channels[index] = value; }
        }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Creates a new collection of UV channels.
        /// </summary>
        public UVChannels()
        {
            channels = new ResizableArray<TVec>[Mesh.UVChannelCount];
            channelsData = new TVec[Mesh.UVChannelCount][];
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Resizes all channels at once.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <param name="trimExess">If exess memory should be trimmed.</param>
        public void Resize(int capacity, bool trimExess = false)
        {
            for (int i = 0; i < Mesh.UVChannelCount; i++)
            {
                if (channels[i] != null)
                {
                    channels[i].Resize(capacity, trimExess);
                }
            }
        }

        #endregion Public Methods
    }
}