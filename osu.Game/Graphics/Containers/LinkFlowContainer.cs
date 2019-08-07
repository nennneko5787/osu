﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Chat;
using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Users;

namespace osu.Game.Graphics.Containers
{
    public class LinkFlowContainer : OsuTextFlowContainer
    {
        public LinkFlowContainer(Action<SpriteText> defaultCreationParameters = null)
            : base(defaultCreationParameters)
        {
        }

        private OsuGame game;

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game)
        {
            // will be null in tests
            this.game = game;
        }

        public void AddLinks(string text, List<Link> links)
        {
            if (string.IsNullOrEmpty(text) || links == null)
                return;

            if (links.Count == 0)
            {
                AddText(text);
                return;
            }

            int previousLinkEnd = 0;

            foreach (var link in links)
            {
                AddText(text.Substring(previousLinkEnd, link.Index - previousLinkEnd));
                AddLink(text.Substring(link.Index, link.Length), link.Url, link.Action, link.Argument);
                previousLinkEnd = link.Index + link.Length;
            }

            AddText(text.Substring(previousLinkEnd));
        }

        public IEnumerable<Drawable> AddLink(string text, string url, LinkAction linkType = LinkAction.External, string linkArgument = null, string tooltipText = null, Action<SpriteText> creationParameters = null)
            => createLink(AddText(text, creationParameters), text, url, linkType, linkArgument, tooltipText);

        public IEnumerable<Drawable> AddLink(string text, Action action, string tooltipText = null, Action<SpriteText> creationParameters = null)
            => createLink(AddText(text, creationParameters), text, tooltipText: tooltipText, action: action);

        public IEnumerable<Drawable> AddLink(IEnumerable<SpriteText> text, string url, LinkAction linkType = LinkAction.External, string linkArgument = null, string tooltipText = null)
        {
            foreach (var t in text)
                AddArbitraryDrawable(t);

            return createLink(text, null, url, linkType, linkArgument, tooltipText);
        }

        public IEnumerable<Drawable> AddUserLink(User user, Action<SpriteText> creationParameters = null)
            => createLink(AddText(user.Username, creationParameters), user.Username, null, LinkAction.OpenUserProfile, user.Id.ToString(), "View profile");

        private IEnumerable<Drawable> createLink(IEnumerable<Drawable> drawables, string text, string url = null, LinkAction linkType = LinkAction.External, string linkArgument = null, string tooltipText = null, Action action = null)
        {
            AddInternal(new DrawableLinkCompiler(drawables.OfType<SpriteText>().ToList())
            {
                RelativeSizeAxes = Axes.Both,
                TooltipText = tooltipText ?? (url != text ? url : string.Empty),
                Action = action ?? (() => game.HandleLink(url, linkType, linkArgument)),
            });

            return drawables;
        }

        // We want the compilers to always be visible no matter where they are, so RelativeSizeAxes is used.
        // However due to https://github.com/ppy/osu-framework/issues/2073, it's possible for the compilers to be relative size in the flow's auto-size axes - an unsupported operation.
        // Since the compilers don't display any content and don't affect the layout, it's simplest to exclude them from the flow.
        public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Where(c => !(c is DrawableLinkCompiler));
    }
}
