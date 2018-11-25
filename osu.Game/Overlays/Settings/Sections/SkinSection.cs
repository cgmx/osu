﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections
{
    public class SkinSection : SettingsSection
    {
        private SkinSettingsDropdown skinDropdown;

        public override string Header => "Skin";

        public override FontAwesome Icon => FontAwesome.fa_paint_brush;

        private readonly Bindable<SkinInfo> dropdownBindable = new Bindable<SkinInfo>();
        private readonly Bindable<int> configBindable = new Bindable<int>();

        private static readonly SkinInfo random_skin_info = new RandomSkinInfo();

        private SkinManager skins;
        private SkinInfo[] usableSkins;

        private readonly Random random = new Random();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SkinManager skins)
        {
            this.skins = skins;

            FlowContent.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                skinDropdown = new SkinSettingsDropdown(),
                new SettingsSlider<double, SizeSlider>
                {
                    LabelText = "Menu cursor size",
                    Bindable = config.GetBindable<double>(OsuSetting.MenuCursorSize),
                    KeyboardStep = 0.01f
                },
                new SettingsSlider<double, SizeSlider>
                {
                    LabelText = "Gameplay cursor size",
                    Bindable = config.GetBindable<double>(OsuSetting.GameplayCursorSize),
                    KeyboardStep = 0.01f
                },
                new SettingsCheckbox
                {
                    LabelText = "Adjust gameplay cursor size based on current beatmap",
                    Bindable = config.GetBindable<bool>(OsuSetting.AutoCursorSize)
                },
            };

            skins.ItemAdded += itemAdded;
            skins.ItemRemoved += itemRemoved;

            config.BindWith(OsuSetting.Skin, configBindable);

            usableSkins = skins.GetAllUsableSkins().ToArray();

            skinDropdown.Bindable = dropdownBindable;
            if (usableSkins.Length > 1)
                skinDropdown.Items = usableSkins.Concat(new[] { random_skin_info });
            else
                skinDropdown.Items = usableSkins;

            // Todo: This should not be necessary when OsuConfigManager is databased
            if (skinDropdown.Items.All(s => s.ID != configBindable.Value))
                configBindable.Value = 0;

            configBindable.BindValueChanged(v => dropdownBindable.Value = skinDropdown.Items.Single(s => s.ID == v), true);
            dropdownBindable.BindValueChanged(v =>
            {
                if (v == random_skin_info)
                    randomizeSkin();
                else
                    configBindable.Value = v.ID;
            });
        }

        private void randomizeSkin()
        {
            int n = usableSkins.Length;
            if (n > 1)
                configBindable.Value = (configBindable.Value + random.Next(n - 1) + 1) % n; // make sure it's always a different one
            else
                configBindable.Value = 0;
        }

        private void itemRemoved(SkinInfo s) => Schedule(() => skinDropdown.Items = skinDropdown.Items.Where(i => i.ID != s.ID).ToArray());
        private void itemAdded(SkinInfo s) => Schedule(() => skinDropdown.Items = skinDropdown.Items.Append(s).ToArray());

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skins != null)
            {
                skins.ItemAdded -= itemAdded;
                skins.ItemRemoved -= itemRemoved;
            }
        }

        private class SizeSlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0.##x");
        }

        private class SkinSettingsDropdown : SettingsDropdown<SkinInfo>
        {
            protected override OsuDropdown<SkinInfo> CreateDropdown() => new SkinDropdownControl { Items = Items };

            private class SkinDropdownControl : DropdownControl
            {
                protected override string GenerateItemText(SkinInfo item) => item.ToString();
            }
        }

        private class RandomSkinInfo : SkinInfo
        {
            public RandomSkinInfo()
            {
                Name = "<Random Skin>";
                ID = -1;
            }

            public override string ToString() => Name;
        }
    }
}
