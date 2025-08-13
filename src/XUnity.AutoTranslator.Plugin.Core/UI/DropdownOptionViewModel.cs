﻿using System;
using System.Collections.Generic;
using UnityEngine;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace XUnity.AutoTranslator.Plugin.Core.UI
{
   internal class DropdownViewModel<TDropdownOptionViewModel, TSelection>
      where TDropdownOptionViewModel : DropdownOptionViewModel<TSelection>
      where TSelection : class
   {
      private Action<TSelection> _onSelected;

      public DropdownViewModel(
         string noSelection,
         string noSelectionTooltip,
         string unselect,
         string unselectTooltip,
         IEnumerable<TDropdownOptionViewModel> options,
         Action<TSelection> onSelected )
      {
         NoSelection = noSelection;
         NoSelectionTooltip = noSelectionTooltip;
         Unselect = unselect;
         UnselectTooltip = unselectTooltip;
         _onSelected = onSelected;

         Options = new List<TDropdownOptionViewModel>();
         foreach( var item in options )
         {
            if( item.IsSelected() )
            {
               CurrentSelection = item;
            }
            Options.Add( item );
         }
      }

      public TDropdownOptionViewModel CurrentSelection { get; set; }

      public List<TDropdownOptionViewModel> Options { get; set; }
      public string NoSelection { get; }
      public string NoSelectionTooltip { get; }
      public string Unselect { get; }
      public string UnselectTooltip { get; }

      public void Select( TDropdownOptionViewModel option )
      {
         if( option?.IsSelected() == true ) return;

         CurrentSelection = option;
         _onSelected?.Invoke( CurrentSelection?.Selection );
      }
   }

   internal class DropdownOptionViewModel<TSelection>
   {
      public DropdownOptionViewModel( string text, Func<bool> isSelected, Func<bool> isEnabled, TSelection selection )
      {
         Text = GUIUtil.CreateContent( text );
         IsSelected = isSelected;
         IsEnabled = isEnabled;
         Selection = selection;
      }

      public virtual GUIContent Text { get; set; }

      public Func<bool> IsEnabled { get; set; }

      public Func<bool> IsSelected { get; set; }

      public TSelection Selection { get; set; }
   }

   internal class TranslatorDropdownOptionViewModel : DropdownOptionViewModel<TranslationEndpointManager>
   {
      private GUIContent _selected;
      private GUIContent _normal;
      private GUIContent _disabled;

      public TranslatorDropdownOptionViewModel( bool fallback, Func<bool> isSelected, TranslationEndpointManager selection ) : base( selection.Endpoint.FriendlyName, isSelected, () => selection.Error == null, selection )
      {
         if( fallback )
         {
            _selected = GUIUtil.CreateContent( selection.Endpoint.FriendlyName, $"<b>当前回退翻译器</b>\n{selection.Endpoint.FriendlyName} 是当前选定的回退翻译器，当主翻译器发生故障时，将用于执行翻译." );
            _disabled = GUIUtil.CreateContent( selection.Endpoint.FriendlyName, $"<b>无法选择回退转换器</b>\n{selection.Endpoint.FriendlyName} 无法选择，因为初始化失败. {selection.Error?.Message}" );
            _normal = GUIUtil.CreateContent( selection.Endpoint.FriendlyName, $"<b>选择回退转换器</b>\n{selection.Endpoint.FriendlyName} 将被选为后备翻译器." );
         }
         else
         {
            _selected = GUIUtil.CreateContent( selection.Endpoint.FriendlyName, $"<b>当前翻译器</b>\n{selection.Endpoint.FriendlyName} 是当前选择的将用于执行翻译的翻译器." );
            _disabled = GUIUtil.CreateContent( selection.Endpoint.FriendlyName, $"<b>无法选择翻译器</b>\n{selection.Endpoint.FriendlyName} 无法选择，因为初始化失败. {selection.Error?.Message}" );
            _normal = GUIUtil.CreateContent( selection.Endpoint.FriendlyName, $"<b>选择翻译器</b>\n{selection.Endpoint.FriendlyName} 将被选为翻译." );
         }
      }

      public override GUIContent Text
      {
         get
         {
            if( Selection.Error != null )
            {
               return _disabled;
            }
            else if( IsSelected() )
            {
               return _selected;
            }
            else
            {
               return _normal;
            }
         }
      }
   }
}
