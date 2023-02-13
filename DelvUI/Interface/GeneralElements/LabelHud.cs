﻿using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using DelvUI.Config;
using DelvUI.Helpers;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Numerics;

namespace DelvUI.Interface.GeneralElements
{
    public class LabelHud : HudElement
    {
        private LabelConfig Config => (LabelConfig)_config;

        public LabelHud(LabelConfig config) : base(config)
        {
        }

        protected override void CreateDrawActions(Vector2 origin)
        {
            // unused
        }

        public override void Draw(Vector2 origin)
        {
            Draw(origin);
        }

        public virtual void Draw(
            Vector2 origin,
            Vector2? parentSize = null,
            GameObject? actor = null,
            string? actorName = null,
            uint? actorCurrentHp = null,
            uint? actorMaxHp = null,
            bool? isPlayerName = null,
            string? title = null)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return;
            }

            string? text = actor == null && actorName == null && actorCurrentHp == null && actorMaxHp == null && title == null ?
                Config.GetText() :
                TextTagsHelper.FormattedText(Config.GetText(), actor, actorName, actorCurrentHp, actorMaxHp, isPlayerName, title);

            DrawLabel(text, origin, parentSize ?? Vector2.Zero, actor);
        }

        protected virtual void DrawLabel(string text, Vector2 parentPos, Vector2 parentSize, GameObject? actor = null)
        {
            Vector2 size;
            Vector2 pos;

            if (Config.UseSystemFont())
            {
                ImGui.PushFont(UiBuilder.DefaultFont);
                size = ImGui.CalcTextSize(text) * Config.GetFontScale();
                pos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(parentPos + Config.Position, -parentSize, Config.FrameAnchor), size, Config.TextAnchor);
                ImGui.PopFont();
            }
            else
            {
                using (FontsManager.Instance.PushFont(Config.FontID))
                {
                    size = ImGui.CalcTextSize(text) * Config.GetFontScale();
                    pos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(parentPos + Config.Position, -parentSize, Config.FrameAnchor), size, Config.TextAnchor);
                }
            }

            DrawLabel(text, pos, size, Color(actor));
        }

        public void DrawLabel(string text, Vector2 pos, Vector2 size, PluginConfigColor color)
        {
            Action<ImDrawListPtr> action = (ImDrawListPtr drawList) =>
            {
                if (Config.ShadowConfig.Enabled)
                {
                    DrawHelper.DrawShadowText(text, pos, color.Base, Config.ShadowConfig.Color.Base, drawList, Config.ShadowConfig.Offset, Config.ShadowConfig.Thickness);
                }

                if (Config.ShowOutline)
                {
                    DrawHelper.DrawOutlinedText(text, pos, color.Base, Config.OutlineColor.Base, drawList);
                }

                if (!Config.ShowOutline && !Config.ShadowConfig.Enabled)
                {
                    drawList.AddText(pos, color.Base, text);
                }
            };

            DrawHelper.DrawInWindow(ID, pos, size, false, true, (drawList) =>
            {
                if (Config.UseSystemFont())
                {
                    ImGui.SetWindowFontScale(Config.GetFontScale());
                    ImGui.PushFont(UiBuilder.DefaultFont);
                    action(drawList);
                    ImGui.PopFont();
                    ImGui.SetWindowFontScale(1);
                }
                else
                {
                    using (FontsManager.Instance.PushFont(Config.FontID))
                    {
                        action(drawList);
                    }
                }
            });
        }

        public virtual PluginConfigColor Color(GameObject? actor = null)
        {
            switch (Config.UseJobColor)
            {
                case true when (actor is Character || actor is BattleNpc battleNpc && battleNpc.ClassJob.Id > 0):
                    return ColorUtils.ColorForActor(actor);
                case true when actor is not Character:
                    return GlobalColors.Instance.NPCFriendlyColor;
            }

            switch (Config.UseRoleColor)
            {
                case true when (actor is Character || actor is BattleNpc battleNpc && battleNpc.ClassJob.Id > 0):
                    {
                        Character? character = actor as Character;
                        return character != null && character.ClassJob.Id > 0 ?
                            GlobalColors.Instance.SafeRoleColorForJobId(character.ClassJob.Id) :
                            ColorUtils.ColorForActor(character);
                    }
                case true when actor is not Character:
                    return GlobalColors.Instance.NPCFriendlyColor;
                default:
                    return Config.Color;
            }
        }

        public virtual (string, Vector2, Vector2, PluginConfigColor) PreCalculate(
            Vector2 origin,
            Vector2? parentSize = null,
            GameObject? actor = null,
            string? actorName = null,
            uint? actorCurrentHp = null,
            uint? actorMaxHp = null,
            bool? isPlayerName = null,
            string? title = null)
        {
            if (!Config.Enabled || Config.GetText() == null)
            {
                return ("", Vector2.Zero, Vector2.Zero, Color(null));
            }

            string? text = actor == null && actorName == null && actorCurrentHp == null && actorMaxHp == null && title == null ?
                Config.GetText() :
                TextTagsHelper.FormattedText(Config.GetText(), actor, actorName, actorCurrentHp, actorMaxHp, isPlayerName, title);

            Vector2 pSize = parentSize ?? Vector2.Zero;
            Vector2 size;
            Vector2 pos;

            if (Config.UseSystemFont())
            {
                ImGui.PushFont(UiBuilder.DefaultFont);
                size = ImGui.CalcTextSize(text) * Config.GetFontScale();
                pos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(origin + Config.Position, -pSize, Config.FrameAnchor), size, Config.TextAnchor);
                ImGui.PopFont();
            }
            else
            {
                using (FontsManager.Instance.PushFont(Config.FontID))
                {
                    size = ImGui.CalcTextSize(text) * Config.GetFontScale();
                    pos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(origin + Config.Position, -pSize, Config.FrameAnchor), size, Config.TextAnchor);
                }
            }

            return (text, pos, size, Color(actor));
        }
    }

    public class IconLabelHud : LabelHud
    {
        private IconLabelConfig Config => (IconLabelConfig)_config;

        public IconLabelHud(IconLabelConfig config) : base(config)
        {
        }

        public override void Draw(Vector2 origin,
            Vector2? parentSize = null,
            GameObject? actor = null,
            string? actorName = null,
            uint? actorCurrentHp = null,
            uint? actorMaxHp = null,
            bool? isPlayerName = null,
            string? title = null)
        {
            string? text = Config.GetText();
            if (!Config.Enabled || text == null)
            {
                return;
            }

            DrawLabel(text, origin, parentSize ?? Vector2.Zero, actor);
        }

        protected override void DrawLabel(string text, Vector2 parentPos, Vector2 parentSize, GameObject? actor = null)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            Vector2 size = ImGui.CalcTextSize(text) * Config.GetFontScale();
            Vector2 pos = Utils.GetAnchoredPosition(Utils.GetAnchoredPosition(parentPos + Config.Position, -parentSize, Config.FrameAnchor), size, Config.TextAnchor);
            ImGui.PopFont();

            DrawHelper.DrawInWindow(ID, pos, size, false, true, (drawList) =>
            {
                ImGui.SetWindowFontScale(Config.GetFontScale());
                ImGui.PushFont(UiBuilder.IconFont);

                PluginConfigColor? color = Color(actor);

                if (Config.ShadowConfig.Enabled)
                {
                    DrawHelper.DrawShadowText(text, pos, color.Base, Config.ShadowConfig.Color.Base, drawList, Config.ShadowConfig.Offset, Config.ShadowConfig.Thickness);
                }

                if (Config.ShowOutline)
                {
                    DrawHelper.DrawOutlinedText(text, pos, color.Base, Config.OutlineColor.Base, drawList);
                }

                if (!Config.ShowOutline && !Config.ShadowConfig.Enabled)
                {
                    drawList.AddText(pos, color.Base, text);
                }

                ImGui.PopFont();
                ImGui.SetWindowFontScale(1);
            });
        }
    }
}