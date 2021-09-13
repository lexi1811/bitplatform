﻿using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace Bit.Client.Web.BlazorUI
{
    public partial class BitPivot
    {
        private string? selectedKey;
        private bool SelectedKeyHasBeenSet;
        private BitLinkSize linkSize = BitLinkSize.Normal;
        private BitLinkFormat linkFormat = BitLinkFormat.Links;
        private BitOverflowBehavior overflowBehavior = BitOverflowBehavior.None;
        private BitPivotItem? SelectedItem { get; set; }
        private List<BitPivotItem> AllItems = new();

        /// <summary>
        /// Default selected key for the pivot
        /// </summary>
        [Parameter] public string? DefaultSelectedKey { get; set; }

        /// <summary>
        /// The content of pivot, It can be Any custom tag
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Overflow behavior when there is not enough room to display all of the links/tabs
        /// </summary>
        [Parameter]
        public BitOverflowBehavior OverflowBehavior
        {
            get => overflowBehavior;
            set
            {
                overflowBehavior = value;
                ClassBuilder.Reset();
            }
        }

        /// <summary>
        /// Pivot link format, display mode for the pivot links
        /// </summary>
        [Parameter]
        public BitLinkFormat LinkFormat
        {
            get => linkFormat;
            set
            {
                linkFormat = value;
                ClassBuilder.Reset();
            }
        }

        /// <summary>
        /// Pivot link size
        /// </summary>
        [Parameter]
        public BitLinkSize LinkSize
        {
            get => linkSize;
            set
            {
                linkSize = value;
                ClassBuilder.Reset();
            }
        }

        /// <summary>
        /// Whether to skip rendering the tabpanel with the content of the selected tab
        /// </summary>
        [Parameter] public bool HeadersOnly { get; set; } = false;

        /// <summary>
        /// Callback for when the selected pivot item is changed
        /// </summary>
        [Parameter] public EventCallback<BitPivotItem> OnLinkClick { get; set; }

        /// <summary>
        /// Key of the selected pivot item. Updating this will override the Pivot's selected item state
        /// </summary>
        [Parameter]
        public string? SelectedKey
        {
            get => selectedKey;
            set
            {
                if (value == selectedKey) return;
                SelectItemByKey(value);
            }
        }

        [Parameter] public EventCallback<string?> SelectedKeyChanged { get; set; }

        internal async Task SelectItem(BitPivotItem item)
        {
            if (SelectedKeyHasBeenSet && SelectedKeyChanged.HasDelegate is false) return;

            SelectedItem?.SetState(false);
            item.SetState(true);

            SelectedItem = item;
            selectedKey = item.Key;

            StateHasChanged();

            await OnLinkClick.InvokeAsync(item);
        }

        internal void RegisterItem(BitPivotItem item)
        {
            if (IsEnabled is false)
            {
                item.IsEnabled = false;
            }

            if (selectedKey is null)
            {
                if (AllItems.Count == 0)
                {
                    item.SetState(true);
                    SelectedItem = item;
                    StateHasChanged();
                }
            }
            else if (selectedKey == item.Key)
            {
                item.SetState(true);
                SelectedItem = item;
                StateHasChanged();
            }

            AllItems.Add(item);
        }

        internal void UnregisterItem(BitPivotItem item)
        {
            AllItems.Remove(item);
        }

        protected override Task OnInitializedAsync()
        {
            selectedKey ??= DefaultSelectedKey;

            return base.OnInitializedAsync();
        }

        protected override string RootElementClass => "bit-pvt";

        protected override void RegisterComponentClasses()
        {
            ClassBuilder.Register(() => LinkSize == BitLinkSize.Large ? $"{RootElementClass}-large-{VisualClassRegistrar()}"
                                      : LinkSize == BitLinkSize.Normal ? $"{RootElementClass}-normal-{VisualClassRegistrar()}"
                                      : string.Empty);

            ClassBuilder.Register(() => LinkFormat == BitLinkFormat.Links ? $"{RootElementClass}-links-{VisualClassRegistrar()}"
                                      : LinkFormat == BitLinkFormat.Tabs ? $"{RootElementClass}-tabs-{VisualClassRegistrar()}"
                                      : string.Empty);

            ClassBuilder.Register(() => OverflowBehavior == BitOverflowBehavior.Menu ? $"{RootElementClass}-menu-{VisualClassRegistrar()}"
                                      : OverflowBehavior == BitOverflowBehavior.Scroll ? $"{RootElementClass}-scroll-{VisualClassRegistrar()}"
                                      : OverflowBehavior == BitOverflowBehavior.None ? $"{RootElementClass}-none-{VisualClassRegistrar()}"
                                      : string.Empty);
        }

        private void SelectItemByKey(string? key)
        {
            var newItem = AllItems.FirstOrDefault(i => i.Key == key);

            if (newItem == null || newItem == SelectedItem || newItem.IsEnabled is false)
            {
                _ = SelectedKeyChanged.InvokeAsync(selectedKey);
                return;
            }

            _ = SelectItem(newItem);
        }
    }
}