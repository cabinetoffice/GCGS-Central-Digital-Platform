using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CO.CDP.OrganisationInformation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLayoutOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    rec RECORD;
                    input_obj jsonb;
                    heading_obj jsonb;
                    button_obj jsonb;
                    new_options jsonb;
                BEGIN
                    FOR rec IN SELECT id, options FROM form_questions WHERE options->'layout' IS NOT NULL
                    LOOP
                        new_options := rec.options;

                        input_obj := jsonb_strip_nulls(jsonb_build_object(
                            'customYesText', rec.options->'layout'->>'customYesText',
                            'customNoText', rec.options->'layout'->>'customNoText',
                            'width', rec.options->'layout'->>'inputWidth',
                            'suffix', rec.options->'layout'->'inputSuffix',
                            'customCssClasses', rec.options->'layout'->>'customCssClasses'
                        ));

                        IF input_obj != '{}'::jsonb THEN
                            new_options := jsonb_set(new_options, '{layout,input}', input_obj);
                        END IF;

                        heading_obj := jsonb_strip_nulls(jsonb_build_object(
                            'size', rec.options->'layout'->>'headingSize',
                            'beforeHeadingContent', rec.options->'layout'->>'beforeTitleContent'
                        ));

                        IF heading_obj != '{}'::jsonb THEN
                            new_options := jsonb_set(new_options, '{layout,heading}', heading_obj);
                        END IF;

                        button_obj := jsonb_strip_nulls(jsonb_build_object(
                            'text', rec.options->'layout'->'button'->>'text',
                            'style', rec.options->'layout'->'button'->>'style',
                            'beforeButtonContent', rec.options->'layout'->>'beforeButtonContent',
                            'afterButtonContent', rec.options->'layout'->>'afterButtonContent'
                        ));

                        IF button_obj != '{}'::jsonb THEN
                            new_options := jsonb_set(new_options, '{layout,button}', button_obj);
                        END IF;

                        UPDATE form_questions SET options = new_options WHERE id = rec.id;
                    END LOOP;
                END $$;
            """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET options = options #- '{layout,customYesText}'
                                         #- '{layout,customNoText}'
                                         #- '{layout,inputWidth}'
                                         #- '{layout,customCssClasses}'
                                         #- '{layout,headingSize}'
                                         #- '{layout,beforeTitleContent}'
                                         #- '{layout,beforeButtonContent}'
                                         #- '{layout,afterButtonContent}'
                                         #- '{layout,inputSuffix}'
                                         #- '{layout,button}'
                    WHERE options->'layout' IS NOT NULL;
                END $$;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,customYesText}', options->'layout'->'input'->'customYesText')
                    WHERE options->'layout'->'input'->'customYesText' IS NOT NULL;

                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,customNoText}', options->'layout'->'input'->'customNoText')
                    WHERE options->'layout'->'input'->'customNoText' IS NOT NULL;

                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,inputWidth}', options->'layout'->'input'->'width')
                    WHERE options->'layout'->'input'->'width' IS NOT NULL;

                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,inputSuffix}', options->'layout'->'input'->'suffix')
                    WHERE options->'layout'->'input'->'suffix' IS NOT NULL;

                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,customCssClasses}', options->'layout'->'input'->'customCssClasses')
                    WHERE options->'layout'->'input'->'customCssClasses' IS NOT NULL;

                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,headingSize}', options->'layout'->'heading'->'size')
                    WHERE options->'layout'->'heading'->'size' IS NOT NULL;

                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,beforeTitleContent}', options->'layout'->'heading'->'beforeHeadingContent')
                    WHERE options->'layout'->'heading'->'beforeHeadingContent' IS NOT NULL;

                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,beforeButtonContent}', options->'layout'->'button'->'beforeButtonContent')
                    WHERE options->'layout'->'button'->'beforeButtonContent' IS NOT NULL;

                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,afterButtonContent}', options->'layout'->'button'->'afterButtonContent')
                    WHERE options->'layout'->'button'->'afterButtonContent' IS NOT NULL;

                    UPDATE form_questions
                    SET options = jsonb_set(options, '{layout,button}', jsonb_build_object(
                        'text', options->'layout'->'button'->>'text',
                        'style', options->'layout'->'button'->>'style'
                    ))
                    WHERE options->'layout'->'button'->>'text' IS NOT NULL
                       OR options->'layout'->'button'->>'style' IS NOT NULL;
                END $$;
            """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    UPDATE form_questions
                    SET options = options #- '{layout,input}'
                                         #- '{layout,heading}'
                    WHERE options->'layout' IS NOT NULL;
                END $$;
            """);
        }
    }
}
