﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Screens.Multi.Components
{
    public class BeatmapTitle : MultiplayerComposite
    {
        private readonly LinkFlowContainer textFlow;

        public BeatmapTitle()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = textFlow = new LinkFlowContainer { AutoSizeAxes = Axes.Both };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            CurrentBeatmap.BindValueChanged(v => updateText(), true);
        }

        private float textSize = OsuSpriteText.FONT_SIZE;

        public float TextSize
        {
            get => textSize;
            set
            {
                if (textSize == value)
                    return;
                textSize = value;

                updateText();
            }
        }

        [Resolved]
        private OsuColour colours { get; set; }

        private void updateText()
        {
            if (!IsLoaded)
                return;

            textFlow.Clear();

            if (CurrentBeatmap.Value == null)
                textFlow.AddText("No beatmap selected", s =>
                {
                    s.TextSize = TextSize;
                    s.Colour = colours.PinkLight;
                });
            else
            {
                textFlow.AddLink(new[]
                {
                    new OsuSpriteText
                    {
                        Text = new LocalisedString((CurrentBeatmap.Value.Metadata.ArtistUnicode, CurrentBeatmap.Value.Metadata.Artist)),
                        TextSize = TextSize,
                    },
                    new OsuSpriteText
                    {
                        Text = " - ",
                        TextSize = TextSize,
                    },
                    new OsuSpriteText
                    {
                        Text = new LocalisedString((CurrentBeatmap.Value.Metadata.TitleUnicode, CurrentBeatmap.Value.Metadata.Title)),
                        TextSize = TextSize,
                    }
                }, null, LinkAction.OpenBeatmap, CurrentBeatmap.Value.OnlineBeatmapID.ToString(), "Open beatmap");
            }
        }
    }
}
