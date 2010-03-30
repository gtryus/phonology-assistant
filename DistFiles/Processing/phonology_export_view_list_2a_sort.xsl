﻿<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:xhtml="http://www.w3.org/1999/xhtml"
exclude-result-prefixes="xhtml"
>

  <!-- phonology_export_view_list_2a_sort.xsl 2010-03-29 -->
  <!-- Make it possible to sort an interactive list by the Phonetic column and also by minimal pair groups. -->

  <!-- Important: If the input is from any other view, copy it with no changes. -->

	<!-- TO DO: Select phonetic/phonological units. -->

	<xsl:output method="xml" version="1.0" encoding="UTF-8" omit-xml-declaration="yes" indent="no" />

	<xsl:variable name="metadata" select="//xhtml:div[@id = 'metadata']" />

	<!-- Phonetic sort options apply to whenever Phonetic is the primary sort field; but even if not, in ungrouped lists, lists grouped by minimal pairs. -->
  <!-- That is, omit the options only in a list with generic groups for which Phonetic is not the primary sort field. -->
	<xsl:variable name="phoneticSortOrder">
    <xsl:if test="//xhtml:table[@class = 'list']//xhtml:td[starts-with(@class, 'Phonetic')]">
      <xsl:variable name="primarySortFieldName" select="$metadata/xhtml:ul[@class = 'sorting']/xhtml:li[@class = 'fieldOrder']/xhtml:ol/xhtml:li[1]/@title" />
      <xsl:if test="$primarySortFieldName = 'Phonetic' or //xhtml:table[@class = 'list'][not(xhtml:tbody[@class = 'group'])] or //xhtml:table[@class = 'list']/xhtml:tbody[@class = 'group']/xhtml:tr[@class = 'heading']/xhtml:th[@class = 'Phonetic pair']">
        <xsl:value-of select="'true'" />
      </xsl:if>
    </xsl:if>
  </xsl:variable>
	<xsl:variable name="phoneticSortOption" select="$metadata/xhtml:ul[@class = 'sorting']/xhtml:li[@class = 'phoneticSortOption']" />

  <!-- A project phonetic inventory file contains digits for sort keys. -->
	<xsl:variable name="projectFolder" select="$metadata/xhtml:ul[@class = 'settings']/xhtml:li[@class = 'projectFolder']" />
	<xsl:variable name="projectPhoneticInventoryFile" select="$metadata/xhtml:ul[@class = 'settings']/xhtml:li[@class = 'projectPhoneticInventoryFile']" />
	<xsl:variable name="projectPhoneticInventoryXML" select="concat($projectFolder, $projectPhoneticInventoryFile)" />
	<xsl:variable name="units" select="document($projectPhoneticInventoryXML)/inventory/units[@type = 'phonetic']" />
	
  <xsl:variable name="maxUnitLength">
    <xsl:for-each select="$units/unit">
      <xsl:sort select="string-length(@literal)" order="descending" data-type="number" />
      <xsl:if test="position() = 1">
        <xsl:value-of select="string-length(@literal)" />
      </xsl:if>
    </xsl:for-each>
  </xsl:variable>

  <!-- Copy all attributes and nodes, and then define more specific template rules. -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <!-- Insert a metadata setting for the rest of the transformations. -->
  <xsl:template match="xhtml:div[@id = 'metadata']/xhtml:ul[@class = 'settings']">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
      <xsl:if test="$phoneticSortOrder = 'true'">
        <li class="phoneticSortOrder" xmlns="http://www.w3.org/1999/xhtml">
          <xsl:value-of select="$phoneticSortOrder" />
        </li>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <!-- Apply or ignore this transformation. -->
  <xsl:template match="xhtml:table[@class = 'list']">
    <xsl:choose>
      <xsl:when test="$phoneticSortOrder = 'true'">
        <xsl:copy>
          <xsl:apply-templates select="@*|node()" />
        </xsl:copy>
      </xsl:when>
      <xsl:otherwise>
        <!-- To ignore all of the following rules, copy instead of apply templates. -->
        <xsl:copy-of select="." />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Remember original order of rows in case Phonetic is not the primary sort field. -->
  <xsl:template match="xhtml:table[@class = 'list']/xhtml:tbody">
    <xsl:variable name="sortOrderFormat" select="translate(count(xhtml:tr[not(@class = 'heading')]), '0123456789', '0000000000')" />
    <xsl:copy>
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="xhtml:tr[@class = 'heading']" />
      <xsl:for-each select="xhtml:tr[not(@class = 'heading')]">
        <xsl:copy>
          <xsl:apply-templates select="@*" />
          <xsl:apply-templates>
            <xsl:with-param name="position" select="position()" />
            <xsl:with-param name="sortOrderFormat" select="$sortOrderFormat" />
          </xsl:apply-templates>
        </xsl:copy>        
      </xsl:for-each>
    </xsl:copy>
  </xsl:template>

  <!-- Phonetic pair heading cells contain a list of two units. -->
  <xsl:template match="xhtml:table[@class = 'list']/xhtml:tbody/xhtml:tr/xhtml:th[@class = 'Phonetic pair']/xhtml:ul">
		<xsl:variable name="sortKey1">
			<xsl:call-template name="phoneticTextToSortKey">
				<xsl:with-param name="text" select="xhtml:li[1]/xhtml:span" />
				<xsl:with-param name="phoneticSortOption" select="$phoneticSortOption" />
				<xsl:with-param name="direction" select="'leftToRight'" />
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="sortKey2">
			<xsl:call-template name="phoneticTextToSortKey">
				<xsl:with-param name="text" select="xhtml:li[2]/xhtml:span" />
				<xsl:with-param name="phoneticSortOption" select="$phoneticSortOption" />
				<xsl:with-param name="direction" select="'leftToRight'" />
			</xsl:call-template>
		</xsl:variable>
		<!-- Make sure the pair of units are in the correct relative order. -->
		<xsl:copy>
			<xsl:choose>
				<xsl:when test="$sortKey2 &lt; $sortKey1">
					<xsl:apply-templates select="xhtml:li[2]" />
					<xsl:apply-templates select="xhtml:li[1]" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="xhtml:li[1]" />
					<xsl:apply-templates select="xhtml:li[2]" />
				</xsl:otherwise>
			</xsl:choose>
		</xsl:copy>
		<!-- Insert the primary sort order list for the group. -->
		<xsl:call-template name="sortOrder">
			<xsl:with-param name="text">
				<xsl:choose>
					<xsl:when test="$sortKey2 &lt; $sortKey1">
						<xsl:value-of select="concat(xhtml:li[2]/xhtml:span, xhtml:li[1]/xhtml:span)" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="concat(xhtml:li[1]/xhtml:span, xhtml:li[2]/xhtml:span)" />
					</xsl:otherwise>
				</xsl:choose>
			</xsl:with-param>
			<xsl:with-param name="direction" select="'leftToRight'" />
		</xsl:call-template>
  </xsl:template>

	<!-- Phonetic cells in heading rows. -->
	<xsl:template match="xhtml:table[@class = 'list']/xhtml:tbody[@class = 'group']/xhtml:tr[@class = 'heading']/xhtml:th[@class = 'Phonetic' or @class = 'Phonetic preceding' or @class = 'Phonetic item' or @class = 'Phonetic following']">
		<xsl:variable name="class" select="@class" />
		<xsl:copy>
			<xsl:apply-templates select="@*" />
			<!-- Enclose the text in a span to separate it from the sort order list. -->
			<span xmlns="http://www.w3.org/1999/xhtml">
				<xsl:value-of select="text()" />
			</span>
			<xsl:call-template name="sortOrder">
				<xsl:with-param name="text" select="text()" />
				<xsl:with-param name="direction">
					<xsl:choose>
						<xsl:when test="$class = 'Phonetic'">
							<xsl:value-of select="'leftToRight'" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$metadata/xhtml:ul[@class = 'sorting']/xhtml:li[@class = 'phoneticSearchSubfieldOrder']/xhtml:ol/xhtml:li[@class = $class]" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:copy>
	</xsl:template>

	<!-- Phonetic cells in data rows. -->
  <xsl:template match="xhtml:table[@class = 'list']/xhtml:tbody/xhtml:tr/xhtml:td[starts-with(@class, 'Phonetic')]">
    <xsl:param name="position" />
    <xsl:param name="sortOrderFormat" />
    <xsl:variable name="class" select="@class" />
		<xsl:copy>
			<xsl:apply-templates select="@*" />
			<!-- Enclose the text in a span to separate it from the sort order list. -->
			<span xmlns="http://www.w3.org/1999/xhtml">
				<xsl:value-of select="text()" />
			</span>
			<xsl:call-template name="sortOrder">
				<xsl:with-param name="text" select="text()" />
				<xsl:with-param name="direction">
					<xsl:choose>
						<xsl:when test="$class = 'Phonetic'">
							<xsl:value-of select="'leftToRight'" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$metadata/xhtml:ul[@class = 'sorting']/xhtml:li[@class = 'phoneticSearchSubfieldOrder']/xhtml:ol/xhtml:li[@class = $class]" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:with-param>
				<xsl:with-param name="position" select="$position" />
				<xsl:with-param name="sortOrderFormat" select="$sortOrderFormat" />
			</xsl:call-template>
		</xsl:copy>
	</xsl:template>

	<xsl:template name="sortOrder">
		<xsl:param name="text" />
		<xsl:param name="direction" />
		<xsl:param name="position" />
		<xsl:param name="sortOrderFormat" />
		<ul class="sortOrder" xmlns="http://www.w3.org/1999/xhtml">
			<li class="placeOfArticulation">
				<xsl:call-template name="phoneticTextToSortKey">
					<xsl:with-param name="text" select="$text" />
					<xsl:with-param name="phoneticSortOption" select="'placeOfArticulation'" />
					<xsl:with-param name="direction" select="$direction" />
				</xsl:call-template>
			</li>
			<li class="mannerOfArticulation">
				<xsl:call-template name="phoneticTextToSortKey">
					<xsl:with-param name="text" select="$text" />
					<xsl:with-param name="phoneticSortOption" select="'mannerOfArticulation'" />
					<xsl:with-param name="direction" select="$direction" />
				</xsl:call-template>
			</li>
			<xsl:if test="$position">
				<li class="exported">
					<xsl:value-of select="format-number($position, $sortOrderFormat)" />
				</li>
			</xsl:if>
		</ul>
	</xsl:template>

	<xsl:template name="phoneticTextToSortKey">
		<xsl:param name="text" />
		<xsl:param name="phoneticSortOption" />
		<xsl:param name="direction" />
		<xsl:param name="unitLength" select="$maxUnitLength" />
		<xsl:variable name="textLength" select="string-length($text)" />
		<xsl:if test="$textLength != 0">
			<xsl:choose>
				<xsl:when test="$unitLength = 0">
					<!-- If no units matched, skip a character, and then continue matching. -->
					<xsl:call-template name="phoneticTextToSortKey">
						<xsl:with-param name="text" select="substring($text, 2)" />
						<xsl:with-param name="phoneticSortOption" select="$phoneticSortOption" />
						<xsl:with-param name="direction" select="$direction" />
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="$unitLength &gt; $textLength">
					<!-- If the current unit length is less than the remaining text length, decrease the length. -->
					<xsl:call-template name="phoneticTextToSortKey">
						<xsl:with-param name="text" select="$text" />
						<xsl:with-param name="phoneticSortOption" select="$phoneticSortOption" />
						<xsl:with-param name="direction" select="$direction" />
						<xsl:with-param name="unitLength" select="$unitLength - 1" />
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<!-- Attempt to match a unit of the current length in the phonetic inventory. -->
					<xsl:variable name="textUnit" select="substring($text, 1, $unitLength)" />
					<xsl:variable name="sortKey" select="$units/unit[@literal = $textUnit]/keys/sortKey[@class = $phoneticSortOption]" />
					<xsl:choose>
						<xsl:when test="string-length($sortKey) != 0">
							<!-- If a unit matches, return its sort key, and then continue matching. -->
							<xsl:if test="$direction != 'rightToLeft'">
								<xsl:value-of select="$sortKey" />
							</xsl:if>
							<xsl:call-template name="phoneticTextToSortKey">
								<xsl:with-param name="text" select="substring($text, $unitLength + 1)" />
								<xsl:with-param name="phoneticSortOption" select="$phoneticSortOption" />
								<xsl:with-param name="direction" select="$direction" />
							</xsl:call-template>
							<xsl:if test="$direction = 'rightToLeft'">
								<xsl:value-of select="$sortKey" />
							</xsl:if>
						</xsl:when>
						<xsl:otherwise>
							<!-- If no phone matched, decrease the length. -->
							<xsl:call-template name="phoneticTextToSortKey">
								<xsl:with-param name="text" select="$text" />
								<xsl:with-param name="phoneticSortOption" select="$phoneticSortOption" />
								<xsl:with-param name="direction" select="$direction" />
								<xsl:with-param name="unitLength" select="$unitLength - 1" />
							</xsl:call-template>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>