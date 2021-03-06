// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// DO NOT EDIT! This file was generated by CustomTasks.DependencyPropertyCodeGen
#include "pch.h"
#include "common.h"
#include "ColorSpectrum.h"

CppWinRTActivatableClassWithDPFactory(ColorSpectrum)

GlobalDependencyProperty ColorSpectrumProperties::s_ColorProperty{ nullptr };
GlobalDependencyProperty ColorSpectrumProperties::s_ComponentsProperty{ nullptr };
GlobalDependencyProperty ColorSpectrumProperties::s_HsvColorProperty{ nullptr };
GlobalDependencyProperty ColorSpectrumProperties::s_MaxHueProperty{ nullptr };
GlobalDependencyProperty ColorSpectrumProperties::s_MaxSaturationProperty{ nullptr };
GlobalDependencyProperty ColorSpectrumProperties::s_MaxValueProperty{ nullptr };
GlobalDependencyProperty ColorSpectrumProperties::s_MinHueProperty{ nullptr };
GlobalDependencyProperty ColorSpectrumProperties::s_MinSaturationProperty{ nullptr };
GlobalDependencyProperty ColorSpectrumProperties::s_MinValueProperty{ nullptr };
GlobalDependencyProperty ColorSpectrumProperties::s_ShapeProperty{ nullptr };

ColorSpectrumProperties::ColorSpectrumProperties()
    : m_colorChangedEventSource{static_cast<ColorSpectrum*>(this)}
{
    EnsureProperties();
}

void ColorSpectrumProperties::EnsureProperties()
{
    if (!s_ColorProperty)
    {
        s_ColorProperty =
            InitializeDependencyProperty(
                L"Color",
                winrt::name_of<winrt::Color>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<winrt::Color>::BoxValueIfNecessary({ 255, 255, 255, 255 }),
                winrt::PropertyChangedCallback(&OnColorPropertyChanged));
    }
    if (!s_ComponentsProperty)
    {
        s_ComponentsProperty =
            InitializeDependencyProperty(
                L"Components",
                winrt::name_of<winrt::ColorSpectrumComponents>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<winrt::ColorSpectrumComponents>::BoxValueIfNecessary(winrt::ColorSpectrumComponents::HueSaturation),
                winrt::PropertyChangedCallback(&OnComponentsPropertyChanged));
    }
    if (!s_HsvColorProperty)
    {
        s_HsvColorProperty =
            InitializeDependencyProperty(
                L"HsvColor",
                winrt::name_of<winrt::float4>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<winrt::float4>::BoxValueIfNecessary({ 0, 0, 1, 1 }),
                winrt::PropertyChangedCallback(&OnHsvColorPropertyChanged));
    }
    if (!s_MaxHueProperty)
    {
        s_MaxHueProperty =
            InitializeDependencyProperty(
                L"MaxHue",
                winrt::name_of<int>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<int>::BoxValueIfNecessary(359),
                winrt::PropertyChangedCallback(&OnMaxHuePropertyChanged));
    }
    if (!s_MaxSaturationProperty)
    {
        s_MaxSaturationProperty =
            InitializeDependencyProperty(
                L"MaxSaturation",
                winrt::name_of<int>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<int>::BoxValueIfNecessary(100),
                winrt::PropertyChangedCallback(&OnMaxSaturationPropertyChanged));
    }
    if (!s_MaxValueProperty)
    {
        s_MaxValueProperty =
            InitializeDependencyProperty(
                L"MaxValue",
                winrt::name_of<int>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<int>::BoxValueIfNecessary(100),
                winrt::PropertyChangedCallback(&OnMaxValuePropertyChanged));
    }
    if (!s_MinHueProperty)
    {
        s_MinHueProperty =
            InitializeDependencyProperty(
                L"MinHue",
                winrt::name_of<int>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<int>::BoxValueIfNecessary(0),
                winrt::PropertyChangedCallback(&OnMinHuePropertyChanged));
    }
    if (!s_MinSaturationProperty)
    {
        s_MinSaturationProperty =
            InitializeDependencyProperty(
                L"MinSaturation",
                winrt::name_of<int>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<int>::BoxValueIfNecessary(0),
                winrt::PropertyChangedCallback(&OnMinSaturationPropertyChanged));
    }
    if (!s_MinValueProperty)
    {
        s_MinValueProperty =
            InitializeDependencyProperty(
                L"MinValue",
                winrt::name_of<int>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<int>::BoxValueIfNecessary(0),
                winrt::PropertyChangedCallback(&OnMinValuePropertyChanged));
    }
    if (!s_ShapeProperty)
    {
        s_ShapeProperty =
            InitializeDependencyProperty(
                L"Shape",
                winrt::name_of<winrt::ColorSpectrumShape>(),
                winrt::name_of<winrt::ColorSpectrum>(),
                false /* isAttached */,
                ValueHelper<winrt::ColorSpectrumShape>::BoxValueIfNecessary(winrt::ColorSpectrumShape::Box),
                winrt::PropertyChangedCallback(&OnShapePropertyChanged));
    }
}

void ColorSpectrumProperties::ClearProperties()
{
    s_ColorProperty = nullptr;
    s_ComponentsProperty = nullptr;
    s_HsvColorProperty = nullptr;
    s_MaxHueProperty = nullptr;
    s_MaxSaturationProperty = nullptr;
    s_MaxValueProperty = nullptr;
    s_MinHueProperty = nullptr;
    s_MinSaturationProperty = nullptr;
    s_MinValueProperty = nullptr;
    s_ShapeProperty = nullptr;
}

void ColorSpectrumProperties::OnColorPropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::OnComponentsPropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::OnHsvColorPropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::OnMaxHuePropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::OnMaxSaturationPropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::OnMaxValuePropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::OnMinHuePropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::OnMinSaturationPropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::OnMinValuePropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::OnShapePropertyChanged(
    winrt::DependencyObject const& sender,
    winrt::DependencyPropertyChangedEventArgs const& args)
{
    auto owner = sender.as<winrt::ColorSpectrum>();
    winrt::get_self<ColorSpectrum>(owner)->OnPropertyChanged(args);
}

void ColorSpectrumProperties::Color(winrt::Color const& value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_ColorProperty, ValueHelper<winrt::Color>::BoxValueIfNecessary(value));
}

winrt::Color ColorSpectrumProperties::Color()
{
    return ValueHelper<winrt::Color>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_ColorProperty));
}

void ColorSpectrumProperties::Components(winrt::ColorSpectrumComponents const& value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_ComponentsProperty, ValueHelper<winrt::ColorSpectrumComponents>::BoxValueIfNecessary(value));
}

winrt::ColorSpectrumComponents ColorSpectrumProperties::Components()
{
    return ValueHelper<winrt::ColorSpectrumComponents>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_ComponentsProperty));
}

void ColorSpectrumProperties::HsvColor(winrt::float4 const& value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_HsvColorProperty, ValueHelper<winrt::float4>::BoxValueIfNecessary(value));
}

winrt::float4 ColorSpectrumProperties::HsvColor()
{
    return ValueHelper<winrt::float4>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_HsvColorProperty));
}

void ColorSpectrumProperties::MaxHue(int value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_MaxHueProperty, ValueHelper<int>::BoxValueIfNecessary(value));
}

int ColorSpectrumProperties::MaxHue()
{
    return ValueHelper<int>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_MaxHueProperty));
}

void ColorSpectrumProperties::MaxSaturation(int value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_MaxSaturationProperty, ValueHelper<int>::BoxValueIfNecessary(value));
}

int ColorSpectrumProperties::MaxSaturation()
{
    return ValueHelper<int>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_MaxSaturationProperty));
}

void ColorSpectrumProperties::MaxValue(int value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_MaxValueProperty, ValueHelper<int>::BoxValueIfNecessary(value));
}

int ColorSpectrumProperties::MaxValue()
{
    return ValueHelper<int>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_MaxValueProperty));
}

void ColorSpectrumProperties::MinHue(int value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_MinHueProperty, ValueHelper<int>::BoxValueIfNecessary(value));
}

int ColorSpectrumProperties::MinHue()
{
    return ValueHelper<int>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_MinHueProperty));
}

void ColorSpectrumProperties::MinSaturation(int value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_MinSaturationProperty, ValueHelper<int>::BoxValueIfNecessary(value));
}

int ColorSpectrumProperties::MinSaturation()
{
    return ValueHelper<int>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_MinSaturationProperty));
}

void ColorSpectrumProperties::MinValue(int value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_MinValueProperty, ValueHelper<int>::BoxValueIfNecessary(value));
}

int ColorSpectrumProperties::MinValue()
{
    return ValueHelper<int>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_MinValueProperty));
}

void ColorSpectrumProperties::Shape(winrt::ColorSpectrumShape const& value)
{
    static_cast<ColorSpectrum*>(this)->SetValue(s_ShapeProperty, ValueHelper<winrt::ColorSpectrumShape>::BoxValueIfNecessary(value));
}

winrt::ColorSpectrumShape ColorSpectrumProperties::Shape()
{
    return ValueHelper<winrt::ColorSpectrumShape>::CastOrUnbox(static_cast<ColorSpectrum*>(this)->GetValue(s_ShapeProperty));
}

winrt::event_token ColorSpectrumProperties::ColorChanged(winrt::TypedEventHandler<winrt::ColorSpectrum, winrt::ColorChangedEventArgs> const& value)
{
    return m_colorChangedEventSource.add(value);
}

void ColorSpectrumProperties::ColorChanged(winrt::event_token const& token)
{
    m_colorChangedEventSource.remove(token);
}
