<?xml version="1.0" encoding="UTF-8"?>
<!-- #############################################################
    # Name:        ExtractCustomFields.xsl
    # Purpose:     Create excerpt of fwdata with custom fields
    #
    # Author:      Greg Trihus <greg_trihus@sil.org>
    #
    # Created:     2014/10/30
    # Copyright:   (c) 2014 SIL International
    # Licence:     <MIT>
    ################################################################-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version="1.0">

  <xsl:output method="xml"/>

  <xsl:template match="node()|@*">
    <xsl:apply-templates select="node()|@*"/>
  </xsl:template>

  <xsl:template match="node()|@*" mode="doCopy">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*" mode="doCopy"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/">
    <xsl:text>&#xa;</xsl:text>
    <xsl:element name="Fw7CustomField">
      <xsl:text>&#xa;</xsl:text>
      <xsl:element name="CustomFields">
        <xsl:for-each select="//CustomField[@class='LexEntry']">
          <xsl:variable name="name" select="@name"/>
          <!-- Only put out names of fields that have contents in at least on entry -->
          <xsl:if test="//Custom[@name=$name]/*[1]">
            <xsl:apply-templates select="." mode="doCopy"/>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
      <xsl:apply-templates select="//rt[@class='LangProject']" mode="doCopy"/>
      <xsl:text>&#xa;</xsl:text>
      <xsl:element name="CustomValues">
        <xsl:apply-templates/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="@*[name()='wsSelector']" mode="doCopy">
    <xsl:variable name="customName" select="parent::*/@name"/>
    <xsl:attribute name="ws">
      <xsl:value-of select="//Custom[@name = $customName]//@ws[1]"/>
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="rt[@class='LangProject']" mode="doCopy">
    <xsl:text>&#xa;</xsl:text>
    <xsl:element name="WritingSystems">
      <xsl:element name="AnalysisWss">
        <xsl:call-template name="wsList">
          <xsl:with-param name="left" select="AnalysisWss/Uni"/>
        </xsl:call-template>
      </xsl:element>
      <xsl:element name="VernWss">
        <xsl:call-template name="wsList">
          <xsl:with-param name="left" select="VernWss/Uni"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="wsList">
    <xsl:param name="left"/>
    <xsl:variable name="first" select="substring-before($left, ' ')"/>
    <xsl:choose>
      <xsl:when test="string-length($first) &gt; 0">
        <xsl:element name="string">
          <xsl:value-of select="$first"/>
        </xsl:element>
        <xsl:call-template name="wsList">
          <xsl:with-param name="left" select="substring-after($left, ' ')"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="string-length($left) &gt; 0">
        <xsl:element name="string">
          <xsl:value-of select="$left"/>
        </xsl:element>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="rt[./Custom[./AStr or ./AUni]]">
    <xsl:text>&#xa;</xsl:text>
    <xsl:copy>
      <xsl:apply-templates select="@*" mode="doCopy"/>
      <xsl:element name="CustomFields">
        <xsl:apply-templates select="Custom[./AStr or ./AUni]" mode="doCopy"/>
      </xsl:element>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="Custom" mode="doCopy">
    <xsl:copy>
      <xsl:attribute name="name">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:attribute name="value">
        <xsl:value-of select="./AStr/Run/text() | ./AUni/text()"/>
      </xsl:attribute>
      <xsl:attribute name="ws">
        <xsl:value-of select="./*/@ws"/>
      </xsl:attribute>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>